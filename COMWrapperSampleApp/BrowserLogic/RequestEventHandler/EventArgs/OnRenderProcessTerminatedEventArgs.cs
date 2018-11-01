// Copyright © 2017 The CefSharp Authors. All rights reserved.
//
// Use of this source code is governed by a BSD-style license that can be found in the LICENSE file.

using CefSharp;

namespace COMWrapperSampleApp.BrowserLogic.RequestEventHandler
{
    public class OnRenderProcessTerminatedEventArgs : BaseRequestEventArgs
    {
        public OnRenderProcessTerminatedEventArgs(IWebBrowser browserControl, IBrowser browser, CefTerminationStatus status)
            : base(browserControl, browser)
        {
            Status = status;
        }

        public CefTerminationStatus Status { get; private set; }
    }
}
