// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

import React from 'react';
import './Popup.css';

export const Popup = ({ isOpen, message, onClose }) => {
    const handleClose = () => {
      onClose();
    };
    if (!isOpen) {
        return null;
    }
    return (
        <div className="popup-container">
            <div className="popup">
                <div className="popup-content">
                    <h2>Error</h2>
                    <p>{message}</p>
                    <button onClick={handleClose}>Close</button>
                </div>
            </div>
        </div>
    );
};

export const PopupContext = React.createContext({
    showPopup: () => {}
  });
