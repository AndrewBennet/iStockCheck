//
//  Utilities.swift
//  iStockcheck
//
//  Created by Andrew Bennet on 07/10/2016.
//  Copyright © 2016 Andrew Bennet. All rights reserved.
//

import Foundation
import Cocoa

extension Set where Element: Hashable {
    public func batch(batchSize: Int) -> Set<Set<Element>> {
        var allSets = Set<Set<Element>>()
        
        var newSet = Set<Element>()
        for item in self {
            newSet.insert(item)
            if newSet.count == batchSize {
                allSets.insert(newSet)
                newSet = Set<Element>()
            }
        }
        if newSet.count != 0 {
            allSets.insert(newSet)
        }
        return allSets
    }
}

class Utilities {
    public static func keysCorrespondingToSelected<TType>(buttons: [TType: NSButton]) -> Set<TType> {
        var selectedKeys = Set<TType>()
        for keyButtonPair in buttons {
            if keyButtonPair.value.state == 1 {
                selectedKeys.insert(keyButtonPair.key)
            }
        }
        return selectedKeys
    }
}
