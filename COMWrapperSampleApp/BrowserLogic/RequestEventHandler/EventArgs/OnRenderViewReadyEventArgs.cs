// Copyright © 2017 The CefSharp Authors. All rights reserved.
//
// Use of this source code is governed by a BSD-style license that can be found in the LICENSE file.

using CefSharp;

namespace COMWrapperSampleApp.BrowserLogic.RequestEventHandler
{
    public class OnRenderViewReadyEventArgs : BaseRequestEventArgs
    {
        public OnRenderViewReadyEventArgs(IWebBrowser browserControl, IBrowser browser) : base(browserControl, browser)
        {
        }
    }
}
