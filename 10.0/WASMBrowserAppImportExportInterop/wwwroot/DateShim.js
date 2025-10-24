export function incrementDay(date) {
  date.setDate(date.getDate() + 1);
  return date;
}

export function logValueAndType(value) {
  console.log("Date:", value)
}
