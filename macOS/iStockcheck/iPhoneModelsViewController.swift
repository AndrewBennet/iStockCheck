//
//  iPhoneModelsViewController.swift
//  iStockcheck
//
//  Created by Andrew Bennet on 04/10/2016.
//  Copyright © 2016 Andrew Bennet. All rights reserved.
//

import Cocoa

class iPhoneModelsViewController: NSViewController {

    @IBOutlet weak var leftStackView: NSStackView!
    @IBOutlet weak var rightStackView: NSStackView!
    var iPhoneModelButtons = [iPhoneModel: NSButton]()
    
    override func viewDidLoad() {
        super.viewDidLoad()
        preferredContentSize = view.fittingSize
        
        for (index, model) in iPhoneModel.GetAllModels().enumerated() {
         
            let stackView: NSStackView = index <= 13 ? leftStackView : rightStackView
            
            let button = NSButton(frame: NSRect(x: 0, y: 0, width: 5, height: 5))
            button.title = model.ToDisplayName()
            button.attributedTitle = NSAttributedString(string: model.ToDisplayName(), attributes: [NSFontAttributeName: NSFont.systemFont(ofSize: 13.0)])
            button.target = self
            button.action = #selector(checkboxDidChange)
            button.setButtonType(.switch)
            
            button.state = UserDefaults.standard.bool(forKey: iPhoneModel.defaultKey(model)) ? 1 : 0
            iPhoneModelButtons[model] = button
            
            stackView.addArrangedSubview(button)
        }
    }
    
    func checkboxDidChange(sender: NSButton) {
        for iphoneCheckboxPair in iPhoneModelButtons {
            UserDefaults.standard.set(iphoneCheckboxPair.value.state == 1, forKey: iPhoneModel.defaultKey(iphoneCheckboxPair.key))
        }
        applicationDelegate.stockChecker.setSearchTerms(models: Utilities.keysCorrespondingToSelected(buttons: iPhoneModelButtons))
    }
}
