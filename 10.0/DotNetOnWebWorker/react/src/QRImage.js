// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

import React, { useState, useContext, useCallback } from 'react'
import { generateQR } from './client';
import { PopupContext } from './Popup.js';

export const QrImage = ({ text, size }) => {
  const [imageUrl, setImageUrl] = useState(null);
  const { showPopup } = useContext(PopupContext);

  const generateAsync = useCallback(async () => {
    if (text && size) {
      try {
        let url = await generateQR(text, size);
        setImageUrl(url);
      } catch (error) {
        showPopup(error.toString());
      }
    }
  }, [text, size, showPopup]);

  return (
    <div>
      <div>
        <button onClick={generateAsync}>Generate QR Code</button>
      </div>
      <div>
        {imageUrl ? (
          <img src={imageUrl} alt="QR Code" />
        ) : null}
      </div>
    </div>
  );
};
