globalThis.counter = 0;

// Takes no parameters and returns nothing.
export function incrementCounter() {
  globalThis.counter += 1;
};

// Returns an int.
export function getCounter() { return globalThis.counter; };

// Takes a parameter and returns nothing. JS doesn't restrict the parameter type, 
// but we can restrict it in the .NET proxy, if desired.
export function logValue(value) { console.log(value); };

// Called for various .NET types to demonstrate mapping to JS primitive types.
export function logValueAndType(value) { console.log(typeof value, value); };
