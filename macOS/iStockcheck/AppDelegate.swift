//
//  AppDelegate.swift
//  iStockcheck
//
//  Created by Andrew Bennet on 01/10/2016.
//  Copyright © 2016 Andrew Bennet. All rights reserved.
//

import Cocoa
import Foundation

@NSApplicationMain
class AppDelegate: NSObject, NSApplicationDelegate, NSUserNotificationCenterDelegate {

    @IBOutlet weak var window: NSWindow!
    @IBOutlet weak var storePanel: NSView!
    
    @IBOutlet weak var pushbulletApiKey: NSSecureTextField!
    @IBOutlet weak var telegramChatId: NSSecureTextField!
    @IBOutlet weak var telegramBotId: NSSecureTextField!
    
    var statusBar = NSStatusBar.system()
    var statusBarItem = NSStatusItem()
    var menu = NSMenu()
    var menuItem = NSMenuItem()
    
    // iPhone model options
    @IBOutlet weak var iPhone7Button: NSButton!
    @IBOutlet weak var iPhone7PlusButton: NSButton!
    @IBOutlet weak var storage32Button: NSButton!
    @IBOutlet weak var storage128Button: NSButton!
    @IBOutlet weak var storage256Button: NSButton!
    @IBOutlet weak var colourJetBlackButton: NSButton!
    @IBOutlet weak var colourBlackButton: NSButton!
    @IBOutlet weak var colourSilverButton: NSButton!
    @IBOutlet weak var colourRoseGoldButton: NSButton!
    @IBOutlet weak var colourGoldButton: NSButton!
    
    var iPhoneModelButtons : [NSButton]!
    var appleStoreButtonsByIdentifier = [String : NSButton]()
    
    var activeModels: Set<iPhoneModel>!
    var activeStoreIdentifiers: Set<String>!
    
    let stockChecker = StockChecker()
    let notifier = Notifier()
    var checkingForStock = false
    
    func applicationDidFinishLaunching(_ aNotification: Notification) {
        // Setup the menu bar for the application
        setupMenubar()
        NSUserNotificationCenter.default.delegate = self
        
        notifier.SendNotification(title: "iStockcheck started", text: "Any detected changes to iPhone stock levels will trigger a notification like this.")
    }
    
    func userNotificationCenter(_ center: NSUserNotificationCenter, shouldPresent notification: NSUserNotification) -> Bool {
        return true
    }
    
    override func awakeFromNib() {
        super.awakeFromNib()
        
        setupStoreButtons()
        
        // Initialise a list of the buttons, and restore their states from user settings
        iPhoneModelButtons = [iPhone7Button, iPhone7PlusButton, storage32Button, storage128Button, storage256Button,
                              colourBlackButton, colourSilverButton]
        restoreButtonStateFromSettings()

        // Determine the models and stores which are active based on the button states
        activeModels = iPhoneModel.MatchingModels(phoneSizes: activePhoneSizes(), colours: activeColours(), storages: activeStorageSizes())
        activeStoreIdentifiers = getActiveStoreIdentifiers()
        
        // Check for new stock on repeat!
        Timer.scheduledTimer(withTimeInterval: 60.0, repeats: true) { _ in
            if !self.checkingForStock {
                self.checkForStockAndNotify()
            }
        }
    }
    
    func checkForStockAndNotify() {
        checkingForStock = true
        stockChecker.CheckForNewStock(modelsToCheck: activeModels, storesToCheck: activeStoreIdentifiers) {
            newStock in
            for stock in newStock {
                self.notifier.SendNotification(ofStock: stock)
            }
            self.checkingForStock = false
        }
    }
    
    func restoreButtonStateFromSettings() {
        for (index, button) in iPhoneModelButtons.enumerated() {
            button.state = UserDefaults.standard.bool(forKey: "iPhoneModelButton\(index)") ? 1 : 0
        }
        for appleStoreButton in appleStoreButtonsByIdentifier {
            appleStoreButton.value.state = UserDefaults.standard.bool(forKey: "appleStoreButton\(appleStoreButton.key)") ? 1 : 0
        }
    }
    
    func setSettingsFromButtonStates() {
        for (index, button) in iPhoneModelButtons.enumerated() {
            UserDefaults.standard.set(button.state == 1, forKey: "iPhoneModelButton\(index)")
        }
        for appleStoreButton in appleStoreButtonsByIdentifier {
            UserDefaults.standard.set(appleStoreButton.value.state == 1, forKey: "appleStoreButton\(appleStoreButton.key)")
        }
    }
    
    func setupStoreButtons() {
        let alphabeticalAppleStores = AppleStore.storesById.sorted{
            return $0.value < $1.value
        }
        
        var previousButtonFrame: NSRect?
        for (index, storeIdNamePair) in alphabeticalAppleStores.enumerated() {
            let button = NSButton(checkboxWithTitle: storeIdNamePair.value, target: nil, action: #selector(checkboxStateDidChange))
            if index != 19 {
                if previousButtonFrame != nil {
                    button.frame.origin.y = previousButtonFrame!.origin.y - 15
                    button.frame.origin.x = previousButtonFrame!.origin.x
                }
                else {
                    button.frame.origin.y = storePanel.frame.maxY - 50
                }
            }
            else {
                button.frame.origin.y = storePanel.frame.maxY - 20
                button.frame.origin.x = storePanel.frame.maxX - 200
            }
            previousButtonFrame = button.frame
            storePanel.addSubview(button)
            
            appleStoreButtonsByIdentifier[storeIdNamePair.key] = button
        }
    }
    
    @IBAction func checkboxStateDidChange(_ sender: NSButton) {
        setSettingsFromButtonStates()
        
        activeModels = iPhoneModel.MatchingModels(phoneSizes: activePhoneSizes(), colours: activeColours(), storages: activeStorageSizes())
        
    }
    
    func activePhoneSizes() -> Set<PhoneSize> {
        var phoneSizes = Set<PhoneSize>()
        if iPhone7Button.state == 1 {
            phoneSizes.insert(.iPhone7)
        }
        if iPhone7PlusButton.state == 1 {
            phoneSizes.insert(.iPhone7Plus)
        }
        return phoneSizes
    }
    
    func activeStorageSizes() -> Set<StorageSize> {
        var storageSizes = Set<StorageSize>()
        if storage32Button.state == 1 {
            storageSizes.insert(.Small)
        }
        if storage128Button.state == 1 {
            storageSizes.insert(.Medium)
        }
        if storage256Button.state == 1 {
            storageSizes.insert(.Large)
        }
        return storageSizes
    }
    
    func activeColours() -> Set<Colour> {
        var colours = Set<Colour>()
        
        func insertColourIfButtonClicked(button: NSButton, colour: Colour){
            if button.state == 1 {
                colours.insert(colour)
            }
        }
        
        insertColourIfButtonClicked(button: colourJetBlackButton, colour: .JetBlack)
        insertColourIfButtonClicked(button: colourBlackButton, colour: .Black)
        insertColourIfButtonClicked(button: colourSilverButton, colour: .Silver)
        insertColourIfButtonClicked(button: colourGoldButton, colour: .Gold)
        insertColourIfButtonClicked(button: colourRoseGoldButton, colour: .RoseGold)
        return colours
    }
    
    func getActiveStoreIdentifiers() -> Set<String> {
        var storeIds = Set<String>()
        for storeButtonKvp in appleStoreButtonsByIdentifier {
            if storeButtonKvp.value.state == 1 {
                storeIds.insert(storeButtonKvp.key)
            }
        }
        return storeIds
    }

    func applicationWillTerminate(_ aNotification: Notification) {
        // Insert code here to tear down your application
    }
    @IBAction func testNotificationsPressed(_ sender: AnyObject) {
        notifier.SendNotification(title: "iStockcheck (Test)", text: "iPhone availability alerts will appear like this.")
    }
    
    @IBAction func pushbulletApiKeyChanged(_ sender: NSSecureTextField) {
        
    }
    
    @objc func setWindowVisible(sender: AnyObject){
        self.window!.orderFront(self)
    }
    
    func setupMenubar() {
        //Add statusBarItem
        statusBarItem = statusBar.statusItem(withLength: -1)
        statusBarItem.menu = menu
        statusBarItem.title = "iS"
        
        //Add menuItem to menu
        menuItem.title = "Settings"
        menuItem.action = #selector(setWindowVisible)
        menuItem.keyEquivalent = ""
        menu.addItem(menuItem)
    }    
}
