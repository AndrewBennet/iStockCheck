//
//  AppleStoresViewController.swift
//  iStockcheck
//
//  Created by Andrew Bennet on 04/10/2016.
//  Copyright © 2016 Andrew Bennet. All rights reserved.
//

import Cocoa

class AppleStoresViewController: NSViewController {

    @IBOutlet weak var rightStackView: NSStackView!
    @IBOutlet weak var leftStackView: NSStackView!
    var appleStoreButtons = [String: NSButton]()
    
    override func viewDidLoad() {
        super.viewDidLoad()
        
        let alphabeticalAppleStores = AppleStore.storesById.sorted{
            return $0.value < $1.value
        }
        
        let totalStoreCount = alphabeticalAppleStores.count
        
        for (index, appleStore) in alphabeticalAppleStores.enumerated() {
            let relevantStackView: NSStackView = index <= totalStoreCount / 2 ? leftStackView : rightStackView
            
            //let button = NSButton(checkboxWithTitle: appleStore.value, target: self, action: #selector(checkboxDidChange))
            
            let button = NSButton(frame: NSRect(x: 0, y: 0, width: 5, height: 5))
            button.attributedTitle = NSAttributedString(string: appleStore.value, attributes: [NSFontAttributeName: NSFont.systemFont(ofSize: 13.0)])
            button.target = self
            button.action = #selector(checkboxDidChange)
            button.setButtonType(.switch)
            
            button.state = UserDefaults.standard.bool(forKey: AppleStore.defaultKey(storeIdentifier: appleStore.key)) ? 1 : 0
            appleStoreButtons[appleStore.key] = button
            
            relevantStackView.addArrangedSubview(button)
        }
    }
    
    func checkboxDidChange(sender: NSButton) {
        for storeCheckboxPair in appleStoreButtons {
            UserDefaults.standard.set(storeCheckboxPair.value.state == 1, forKey: AppleStore.defaultKey(storeIdentifier: storeCheckboxPair.key))
        }
        applicationDelegate.stockChecker.setSearchTerms(stores: Utilities.keysCorrespondingToSelected(buttons: appleStoreButtons))
    }

}
