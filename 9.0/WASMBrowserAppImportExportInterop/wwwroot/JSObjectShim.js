export function createObject() {
  return {
    name: "Example JS Object",
    answer: 41,
    question: null,
    summarize: function () {
      return `Question: "${this.question}" Answer: ${this.answer}`;
    }
  };
}

export function incrementAnswer(object) {
  object.answer += 1;
  // Don't return the modified object, since the reference is modified.
}

// Proxy an instance method call.
export function summarize(object) {
  return object.summarize();
}
