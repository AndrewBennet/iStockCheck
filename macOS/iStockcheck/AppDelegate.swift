//
//  AppDelegate.swift
//  iStockcheck
//
//  Created by Andrew Bennet on 04/10/2016.
//  Copyright © 2016 Andrew Bennet. All rights reserved.
//

import Cocoa

var applicationDelegate = NSApp.delegate as! AppDelegate

@NSApplicationMain
class AppDelegate: NSObject, NSApplicationDelegate, NSUserNotificationCenterDelegate {

    var statusBarItem = NSStatusBar.system().statusItem(withLength: -1)
    var mainWindowController: MainWindowController?
    
    let stockChecker = StockChecker()
    let notifier = Notifier()
    let stockCheckFrequencySeconds = 60.0

    func applicationDidFinishLaunching(_ aNotification: Notification) {
        NSUserNotificationCenter.default.delegate = self
        setupMenubar()
        
        notifier.sendNotification(title: "iStockcheck started", text: "Any detected changes to iPhone stock levels will trigger a notification like this.")
        
        // Load the selected models and stores
        var selectedStores = Set<String>()
        for appleStoreId in AppleStore.storesById.keys {
            if UserDefaults.standard.bool(forKey: AppleStore.defaultKey(storeIdentifier: appleStoreId)) {
                selectedStores.insert(appleStoreId)
            }
        }
        
        var selectedModels = Set<iPhoneModel>()
        for model in iPhoneModel.GetAllModels() {
            if UserDefaults.standard.bool(forKey: iPhoneModel.defaultKey(model)) {
                selectedModels.insert(model)
            }
        }
        
        stockChecker.setSearchTerms(models: selectedModels)
        stockChecker.setSearchTerms(stores: selectedStores)
        
        Timer.scheduledTimer(timeInterval: stockCheckFrequencySeconds, target: self, selector: #selector(checkForStockAndNotify), userInfo: nil, repeats: true)
    }
    
    func checkForStockAndNotify() {
        // We don't want to start checking for stock if there are still requests going on
        if !self.stockChecker.stockCheckInProcess() {
            
            // Check for stock...
            self.stockChecker.checkForNewStock {
                //... for each new stock found, send a notification
                self.notifier.sendNotification(ofStock: $0)
            }
        }
    }
    
    func terminate() {
        NSApp.terminate(self)
    }

    @objc func setWindowVisible() {
        mainWindowController?.window?.makeKeyAndOrderFront(self)
        NSApp.activate(ignoringOtherApps: true)
    }
    
    func userNotificationCenter(_ center: NSUserNotificationCenter, shouldPresent notification: NSUserNotification) -> Bool {
        // Show notifications even when the app is in focus
        return true
    }

    func setupMenubar() {
        let settingsMenuItem = NSMenuItem()
        settingsMenuItem.title = "Settings"
        settingsMenuItem.action = #selector(self.setWindowVisible)
        settingsMenuItem.keyEquivalent = "S"
        
        let quitMenuItem = NSMenuItem()
        quitMenuItem.title = "Quit"
        quitMenuItem.action = #selector(self.terminate)
        quitMenuItem.keyEquivalent = "Q"
        
        let menu = NSMenu()
        menu.addItem(settingsMenuItem)
        menu.addItem(quitMenuItem)
        
        statusBarItem.menu = menu
        statusBarItem.title = "iS"
    }
}

