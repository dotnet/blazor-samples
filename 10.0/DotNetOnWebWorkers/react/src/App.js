// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

import './App.css';
import React, { useState, useCallback } from 'react';
import debounce from 'lodash.debounce';
import { QrImage } from './QRImage.js';
import { Popup, PopupContext } from './Popup.js';

function App() {
  const initText = "Type text to generate QR";
  const initSize = 5;
  const [text, setText] = useState(initText);
  const [size, setSize] = useState(initSize);
  const debouncedSetText = debounce(e => setText(e.target.value), 100);
  const debouncedSetSize = debounce(e => setSize(e.target.value), 100);

  const [popupMessage, setPopupMessage] = useState(null);
  const [isPopupOpen, setIsPopupOpen] = useState(false);

  const showPopup = useCallback((message) => {
    setPopupMessage(message);
    setIsPopupOpen(true);
  }, []);

  const closePopup = useCallback(() => {
    setIsPopupOpen(false);
  }, []);

  return (
    <PopupContext.Provider value={{ showPopup }}>
      <div className="App">
        <header className="App-header">
        <div className="GitHub-project-info">
          <p>
            This demo is available on GitHub:
            <br />
            <a href="https://github.com/ilonatommy/reactWithDotnetOnWebWorker">GitHub Project Link</a>
          </p>
          </div>
          <div>
            <div className="main-content">
              <p>
                Generate a QR from text:
                <br />
                <input type="text" placeholder={initText} onChange={debouncedSetText} />
                <br />
                Set size of QR (in pixels):
                <br />
                <input type="number" placeholder={initSize} onChange={debouncedSetSize} />
              </p>
              <div>
                <QrImage text={text} size={size} id="qrImage" />
                <Popup isOpen={isPopupOpen} message={popupMessage} onClose={closePopup} />
              </div>
              <p>
                QRCoder from <a href="https://github.com/codebude/QRCoder">https://github.com/codebude/QRCoder</a>
              </p>
              <p>
                WASM QR generator from <a href="https://github.com/maraf/dotnet-wasm-react">https://github.com/maraf/dotnet-wasm-react</a>
              </p>
            </div>
          </div>
        </header>
      </div>
      </PopupContext.Provider>
  );
}

export default App;
