export function returnArrayAsync() {
  DotNet.invokeMethodAsync('BlazorSample', 'ReturnArrayAsync')
    .then(data => {
      console.log(data);
    });
}

export function addHandlers() {
  const btn = document.getElementById("btn");
  btn.addEventListener("click", returnArrayAsync);
}
