//
//  NotificationsViewController.swift
//  iStockcheck
//
//  Created by Andrew Bennet on 04/10/2016.
//  Copyright © 2016 Andrew Bennet. All rights reserved.
//

import Cocoa

class NotificationsViewController: NSViewController {
    
    @IBOutlet weak var pushbulletApiKey: NSTextField!
    @IBOutlet weak var telegramBotId: NSTextField!
    @IBOutlet weak var telegramChatId: NSTextField!
    
    let pushbulletApiKeyKey = "pushbulleyApiKey"
    let telegramBotIdKey = "telegramBotId"
    let telegramChatIdKey = "telegramChatId"
    
    override func viewDidAppear() {
        pushbulletApiKey.stringValue = UserDefaults.standard.string(forKey: pushbulletApiKeyKey) ?? ""
        telegramBotId.stringValue = UserDefaults.standard.string(forKey: telegramBotIdKey) ?? ""
        telegramChatId.stringValue = UserDefaults.standard.string(forKey: telegramChatIdKey) ?? ""
    }

    @IBAction func saveNotificationMethods(_ sender: AnyObject) {
        UserDefaults.standard.set(pushbulletApiKey.stringValue, forKey: pushbulletApiKeyKey)
        UserDefaults.standard.set(telegramBotId.stringValue, forKey: telegramBotIdKey)
        UserDefaults.standard.set(telegramChatId.stringValue, forKey: telegramChatIdKey)
        
        applicationDelegate .notifier.pushbulletApiKey = pushbulletApiKey.stringValue
        applicationDelegate.notifier.telegramBotId = telegramBotId.stringValue
        applicationDelegate.notifier.telegramChatId = telegramChatId.stringValue
    }

    @IBAction func testNotifications(_ sender: AnyObject) {
        saveNotificationMethods(self)
        applicationDelegate.notifier.sendNotification(title: "iStockcheck Test", text: "Test notification sent at \(Date())")
    }
    
}
