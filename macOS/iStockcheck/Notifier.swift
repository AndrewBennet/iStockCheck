//
//  Notifier.swift
//  iStockcheck
//
//  Created by Andrew Bennet on 02/10/2016.
//  Copyright © 2016 Andrew Bennet. All rights reserved.
//

import Foundation

class Notifier {
    
    var sendOSNotification = true
    
    func SendNotification(ofStock stock: Stock) {
        if sendOSNotification {
            SendOSNotification(title: "New Stock Available", text: "\(stock.model.ToDisplayName()) now available at \(AppleStore.storesById[stock.storeIdentifier]!)")
        }
    }
    
    func SendNotification(title: String, text: String){
        if sendOSNotification {
            SendOSNotification(title: title, text: text)
        }
    }
    
    private func SendOSNotification(title: String, text: String) {
        let notification = NSUserNotification()
        notification.title = title
        notification.informativeText = text
        notification.soundName = NSUserNotificationDefaultSoundName
        notification.hasActionButton = true
        notification.actionButtonTitle = "View"
        NSUserNotificationCenter.default.deliver(notification)
    }
}
