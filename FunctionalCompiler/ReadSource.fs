module ReadSource

let sourceCode path =
    System.IO.File.ReadAllText path

