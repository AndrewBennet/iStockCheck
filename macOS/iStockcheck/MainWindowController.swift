//
//  MainWindowController.swift
//  iStockcheck
//
//  Created by Andrew Bennet on 04/10/2016.
//  Copyright © 2016 Andrew Bennet. All rights reserved.
//

import Cocoa
import Foundation

class MainWindowController: NSWindowController {

    override func windowDidLoad() {
        super.windowDidLoad()
        
        applicationDelegate.mainWindowController = self
        
        window?.makeKeyAndOrderFront(self)
        NSApp.activate(ignoringOtherApps: true)
    }
}
