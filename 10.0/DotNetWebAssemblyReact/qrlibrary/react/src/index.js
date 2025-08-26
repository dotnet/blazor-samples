import React from 'react';
import { generate } from 'dotnet-qr';

export const QrImage = ({ text, relativePath }) => {
  const [imageSrc, setImageSrc] = React.useState(undefined);
  React.useEffect(() => {
    async function generateAsync() {
      if (text) {
        var image = await generate(text, 10);
        setImageSrc("data:image/bmp;base64, " + image);
      } else {
        setImageSrc(null);
      }
    }

    generateAsync();
  }, [text]);

  if (imageSrc) {
    return (<img src={imageSrc} />);
  }

  if (imageSrc === null) {
    return;
  }

  return (
    <i>Loading...</i>
  );
}
