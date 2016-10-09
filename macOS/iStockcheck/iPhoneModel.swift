//
//  iPhoneModel.swift
//  iStockcheck
//
//  Created by Andrew Bennet on 01/10/2016.
//  Copyright © 2016 Andrew Bennet. All rights reserved.
//

import Foundation

public enum PhoneSize : Int {
    case iPhone7 = 0, iPhone7Plus = 15
}

public enum Colour : Int {
    case JetBlack = 0, Black = 3, Silver = 6, Gold = 9, RoseGold = 12
}

public enum StorageSize : Int {
    case Small = 0, Medium = 1, Large = 2
}

public class iPhoneModel : Hashable {
    
    public init(phoneSize: PhoneSize, storageSize: StorageSize, colour: Colour) {
        self.PhoneSize = phoneSize;
        self.StorageSize = storageSize;
        self.Colour = colour;
    }
    
    public let PhoneSize : PhoneSize
    
    public let StorageSize : StorageSize
    
    public let Colour : Colour
    
    static let ModelIdentifiers : [String] = [
        //32  128  256
        // == iPhone 7 ==
        "??", "96", "9C", // jet black
        "8X", "92", "97", // black
        "8Y", "93", "98", // silver
        "90", "94", "99", // gold
        "91", "95", "9A", // rose gold
        // == iPhone 7 Plus ==
        "??", "4V", "51", // jet black
        "QM", "4M", "4W", // black
        "QN", "4P", "4X", // silver
        "QP", "4Q", "4Y", // gold
        "QQ", "4U", "50" // rose gold
    ]
    
    public var hashValue: Int {
        get{
            return PhoneSize.rawValue + Colour.rawValue + StorageSize.rawValue
        }
    }
    
    public static func ==(lhs: iPhoneModel, rhs: iPhoneModel) -> Bool {
        return lhs.hashValue == rhs.hashValue
    }
    
    public func ToIdentifier() -> String {
        return "MN\(iPhoneModel.ModelIdentifiers[self.hashValue])2B/A";
    }
    
    public func ToDisplayName() -> String {
        var displayName = "iPhone 7"
        if PhoneSize == .iPhone7Plus{
            displayName += " Plus"
        }
        displayName += ", "
        switch self.Colour{
        case .JetBlack:
            displayName += "Jet Black"
        case .Black:
            displayName += "Black"
        case .Silver:
            displayName += "Silver"
        case .Gold:
            displayName += "Gold"
        case .RoseGold:
            displayName += "Rose Gold"
        }
        displayName += ", "
        switch self.StorageSize{
        case .Small:
            displayName += "32GB"
        case .Medium:
            displayName += "128GB"
        case .Large:
            displayName += "256GB"
        }
        return displayName
    }
    
    public static func GetAllModels() -> [iPhoneModel] {
        var models = [iPhoneModel]()
        for phoneSize: PhoneSize in [.iPhone7, .iPhone7Plus] {
            for colour: Colour in [.JetBlack, .Black, .Silver, .Gold, .RoseGold] {
                for storage: StorageSize in [.Small, .Medium, .Large] {
                    if colour != .JetBlack || storage != .Small {
                        models.append(iPhoneModel(phoneSize: phoneSize, storageSize: storage, colour: colour))
                    }
                }
            }
        }
        return models
    }
    
    public static func MatchingModels(phoneSizes: Set<PhoneSize>, colours: Set<Colour>, storages: Set<StorageSize>) -> Set<iPhoneModel> {
        var models = Set<iPhoneModel>()
        for phoneSize in phoneSizes{
            for colour in colours {
                for storage in storages{
                    if colour != .JetBlack || storage != .Small {
                        models.insert(iPhoneModel(phoneSize: phoneSize, storageSize: storage, colour: colour))
                    }
                }
            }
        }
        return models
    }
    
    
    public static func defaultKey(_ model: iPhoneModel) -> String {
        return "iPhoneModelButton_\(model.ToDisplayName())"
    }
}
