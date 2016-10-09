//
//  AppleStore.swift
//  iStockcheck
//
//  Created by Andrew Bennet on 02/10/2016.
//  Copyright © 2016 Andrew Bennet. All rights reserved.
//

import Foundation

class AppleStore {
    
    public static let storesById = [
        "R659" : "Apple Watch at Selfridges",
        "R227" : "Bentall Centre",
        "R113" : "Bluewater",
        "R340" : "Braehead",
        "R163" : "Brent Cross",
        "R496" : "Bromley",
        "R135" : "Buchanan Street",
        "R118" : "Bullring",
        "R252" : "Cabot Circus",
        "R391" : "Chapelfield",
        "R244" : "Churchill Square",
        "R245" : "Covent Garden",
        "R393" : "Cribbs Causeway",
        "R545" : "Drake Circus",
        "R341" : "Eldon Square",
        "R482" : "Festival Place",
        "R270" : "Grand Arcade",
        "R308" : "Highcross",
        "R242" : "Lakeside",
        "R239" : "Liverpool ONE",
        "R215" : "Manchester Arndale",
        "R153" : "Meadowhall",
        "R423" : "Metrocentre",
        "R269" : "Milton Keynes",
        "R328" : "Princes Street",
        "R279" : "Princesshay",
        "R092" : "Regent Street",
        "R335" : "SouthGate",
        "R334" : "St David's 2",
        "R410" : "Stratford City",
        "R176" : "The Oracle",
        "R255" : "Touchwood Centre",
        "R136" : "Trafford Centre",
        "R372" : "Trinity Leeds",
        "R363" : "Union Square",
        "R313" : "Victoria Square",
        "R527" : "Watford",
        "R174" : "WestQuay",
        "R226" : "White City"
    ]
    
    public static func defaultKey(storeIdentifier: String) -> String {
        return "appleStoreButton_\(storeIdentifier)"
    }
}
