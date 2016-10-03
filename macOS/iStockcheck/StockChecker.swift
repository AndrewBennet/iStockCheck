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
    
    var existingStock = Set<Stock>()
    
    public static let appleAvailabilityUrl = URL(string: "https://reserve.cdn-apple.com/GB/en_GB/reserve/iPhone/availability.json")!

    func CheckForNewStock(modelsToCheck: Set<iPhoneModel>, storesToCheck: Set<String>, differentStockCallback: @escaping (Set<Stock>) -> Void) {
        CheckStock(modelsToCheck: modelsToCheck, storesToCheck: storesToCheck) { newStock in
            let differingStock = newStock.subtracting(self.existingStock)
            self.existingStock = newStock
            differentStockCallback(differingStock)
        }
    }
    
    func CheckStock(modelsToCheck: Set<iPhoneModel>, storesToCheck: Set<String>, callback: @escaping (Set<Stock>) -> Void) {
        var stock = Set<Stock>()
        Alamofire.request(StockChecker.appleAvailabilityUrl).responseJSON {
            response in
            let jsonResponse = JSON(response.result.value)
            for store in storesToCheck {
                for model in modelsToCheck {
                    if let stockString = jsonResponse[store][model.ToIdentifier()].string,
                        stockString != "NONE" {
                        stock.insert(Stock(model: model, storeIdentifier: store))
                    }
                }
            }
            callback(stock)
        }
    }
}
