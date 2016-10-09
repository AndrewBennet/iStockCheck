//
//  StockChecker.swift
//  iStockcheck
//
//  Created by Andrew Bennet on 01/10/2016.
//  Copyright © 2016 Andrew Bennet. All rights reserved.
//

import Foundation
import Alamofire
import SwiftyJSON

struct Stock : Hashable {
    let model: iPhoneModel
    let storeIdentifier: String
    
    init(model: iPhoneModel, storeIdentifier: String){
        self.model = model
        self.storeIdentifier = storeIdentifier
    }
    
    var hashValue: Int {
        return storeIdentifier.hashValue &* 30 + model.hashValue
    }
    
    public static func ==(lhs: Stock, rhs: Stock) -> Bool {
        return lhs.hashValue == rhs.hashValue
    }
}

class StockChecker {
    
    private var existingStock = Set<Stock>()
    
    private static let applePickupUrl = "https://www.apple.com/uk/shop/retail/pickup-message"
    
    private var modelsToSearchFor = Set<iPhoneModel>()
    private var storesToSearchIn = Set<String>()
    private var awaitedResponseCount = 0
    
    public func stockCheckInProcess() -> Bool {
        return awaitedResponseCount != 0
    }
    
    public func setSearchTerms(models: Set<iPhoneModel>) {
        existingStock.removeAll(keepingCapacity: true)
        modelsToSearchFor = models
    }
    
    public func setSearchTerms(stores: Set<String>){
        existingStock.removeAll(keepingCapacity: true)
        storesToSearchIn = stores
    }
    
    public func checkForNewStock(newStockCallback: @escaping (Stock) -> Void) {
        for store in storesToSearchIn {
            for modelBatch in modelsToSearchFor.batch(batchSize: 10) {
                queryForModelBatch(modelBatch, atStore: store, respond: newStockCallback)
            }
        }
    }
    
    private func queryForModelBatch(_ modelBatch: Set<iPhoneModel>, atStore store: String, respond: @escaping ((Stock) -> Void)) {
        // Fire off the web request
        awaitedResponseCount += 1
        Alamofire.request(StockChecker.buildStockCheckUrl(storeIdentifier: store, models: modelBatch)).responseJSON {
            
            self.awaitedResponseCount -= 1
            
            // Load the response into JSON, and look for the "stores" array
            let jsonResponse = JSON($0.result.value)
            if let storeJson = jsonResponse["body"]["stores"].array?.first{
                
                // For each of the models we searched for, check whether there was stock
                for model in modelBatch {
                    
                    let thisStock = Stock(model: model, storeIdentifier: store)
                    
                    // If there is a store pickup quote for this model, and it doesn't contain the word
                    // 'unavailable', we consider this indicative of stock present. If we don't already know
                    // about this, we record this stock and perform the callback.
                    let pickupQuote = storeJson["partsAvailability"][model.ToIdentifier()]["storePickupQuote"].string,
                        stockAvailable = pickupQuote != nil && !pickupQuote!.lowercased().contains("unavailable")
                    
                    if stockAvailable {
                        if !self.existingStock.contains(thisStock) {
                            self.existingStock.insert(thisStock)
                            respond(thisStock)
                        }
                    }
                    else {
                        self.existingStock.remove(thisStock)
                    }
                }
            }
        }
    }
    
    private static func buildStockCheckUrl(storeIdentifier: String, models: Set<iPhoneModel>) -> URL {
        var query = ""
        for (index, model) in models.enumerated() {
            query += "parts.\(index)=\(model.ToIdentifier().addingPercentEncoding(withAllowedCharacters: .urlHostAllowed)!)&"
        }
        query += "store=\(storeIdentifier)"
        return URL(string: "\(applePickupUrl)?\(query)")!
    }
}
