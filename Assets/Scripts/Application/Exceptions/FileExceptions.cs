using System;

namespace TrekVRApplication {

    class FileFormatException : Exception {
        public FileFormatException() : base() { }
        public FileFormatException(string message) : base(message) { }
    }

    class FileNotFoundException : Exception {
        public FileNotFoundException() : base() { }
        public FileNotFoundException(string message) : base(message) { }
    }

    class FileNotSpecifiedException : Exception {
        public FileNotSpecifiedException() : base() { }
        public FileNotSpecifiedException(string message) : base(message) { }
    }

    class FileReadException : Exception {
        public FileReadException() : base() { }
        public FileReadException(string message) : base(message) { }
    }

}
