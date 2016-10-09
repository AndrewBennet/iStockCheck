//
//  Notifier.swift
//  iStockcheck
//
//  Created by Andrew Bennet on 02/10/2016.
//  Copyright © 2016 Andrew Bennet. All rights reserved.
//

import Foundation
import Alamofire
import SwiftyJSON

class Notifier {
    
    var sendOSNotification = true
    public var pushbulletApiKey: String?
    public var telegramBotId: String?
    public var telegramChatId: String?
    
    func sendNotification(ofStock stock: Stock) {
        sendNotification(title: "New Stock Available", text: "\(stock.model.ToDisplayName()) now available at \(AppleStore.storesById[stock.storeIdentifier]!)")
    }
    
    func sendNotification(title: String, text: String){
        if sendOSNotification {
            sendOSNotification(title: title, text: text)
        }
        if pushbulletApiKey != nil {
            sendPushbulletNotification(title: title, text: text)
        }
        if telegramChatId != nil && telegramBotId != nil {
            sendTelegramNotification(text: text)
        }
    }
    
    private func sendOSNotification(title: String, text: String) {
        let notification = NSUserNotification()
        notification.title = title
        notification.informativeText = text
        notification.soundName = NSUserNotificationDefaultSoundName
        notification.hasActionButton = true
        notification.actionButtonTitle = "View"
        NSUserNotificationCenter.default.deliver(notification)
    }
    
    private func sendPushbulletNotification(title: String, text: String) {
        let parameters: Parameters = [
            "body": "\(text)",
            "title": "\(title)",
            "type": "note"
        ]
        
        let _ = Alamofire.request("https://api.pushbullet.com/v2/pushes", method: .post, parameters: parameters, encoding: JSONEncoding.default, headers: HTTPHeaders(dictionaryLiteral: ("Access-Token", pushbulletApiKey!)))
    }
    
    private func sendTelegramNotification(text: String) {
        let telegramMessageUrl = "https://api.telegram.org/bot\(telegramBotId!)/sendMessage?chat_id=\(telegramChatId!)&text=\(text.addingPercentEncoding(withAllowedCharacters: .urlHostAllowed)!)"
        let _ = Alamofire.request(telegramMessageUrl)
    }
}
