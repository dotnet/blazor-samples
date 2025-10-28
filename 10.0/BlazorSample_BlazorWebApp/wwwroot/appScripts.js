window.convertArray = (win1251Array) => {
  var win1251decoder = new TextDecoder('windows-1251');
  var bytes = new Uint8Array(win1251Array);
  var decodedArray = win1251decoder.decode(bytes);
  return decodedArray;
};

window.returnObjectReference = () => {
  return {
    unmarshalledFunctionReturnBoolean: function (fields) {
      const name = Blazor.platform.readStringField(fields, 0);
      const year = Blazor.platform.readInt32Field(fields, 8);
      return name === "Brigadier Alistair Gordon Lethbridge-Stewart" &&
        year === 1968;
    },
    unmarshalledFunctionReturnString: function (fields) {
      const name = Blazor.platform.readStringField(fields, 0);
      const year = Blazor.platform.readInt32Field(fields, 8);
      return BINDING.js_string_to_mono_string(`Hello, ${name} (${year})!`);
    }
  };
}

window.displayTickerAlert1 = (symbol, price) => {
  alert(`${symbol}: $${price}!`);
};

window.displayTickerAlert2 = (symbol, price) => {
  if (price < 20) {
    alert(`${symbol}: $${price}!`);
    return "Alert!";
  } else {
    return "No Alert";
  }
};

window.setElementClass = (element, className) => {
  var myElement = element;
  myElement.classList.add(className);
}

window.sayHello1 = (dotNetHelper) => {
  return dotNetHelper.invokeMethodAsync('GetHelloMessage');
};

window.sayHello2 = (dotNetHelper, name) => {
  return dotNetHelper.invokeMethodAsync('GetHelloMessage', name);
};

window.updateMessageCaller = (dotnetHelper) => {
  dotnetHelper.invokeMethodAsync('UpdateMessageCaller');
  dotnetHelper.dispose();
}

window.downloadFileFromStream = async (fileName, contentStreamReference) => {
  const arrayBuffer = await contentStreamReference.arrayBuffer();
  const blob = new Blob([arrayBuffer]);
  const url = URL.createObjectURL(blob);
  const anchorElement = document.createElement('a');
  anchorElement.href = url;
  anchorElement.download = fileName ?? '';
  anchorElement.click();
  anchorElement.remove();
  URL.revokeObjectURL(url);
}
window.triggerFileDownload = (fileName, url) => {
  const anchorElement = document.createElement('a');
  anchorElement.href = url;
  anchorElement.download = fileName ?? '';
  anchorElement.click();
  anchorElement.remove();
}

window.setSource = async (elementId, stream, contentType, title) => {
	const arrayBuffer = await stream.arrayBuffer();
	let blobOptions = {};
	if (contentType) {
	  blobOptions['type'] = contentType;
	}
	const blob = new Blob([arrayBuffer], blobOptions);
	const url = URL.createObjectURL(blob);
	const element = document.getElementById(elementId);
	element.title = title;
	element.onload = () => {
	  URL.revokeObjectURL(url);
	}
	element.src = url;
}

window.scrollElementIntoView = (element) => {
	element.scrollIntoView();
	return element.getBoundingClientRect().top;
}
