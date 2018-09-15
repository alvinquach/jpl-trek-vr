using System;

class DataElevationModelFormatException : Exception {
    public DataElevationModelFormatException() : base() { }
    public DataElevationModelFormatException(string message) : base(message) { }
}

class DataElevationModelNotFoundException : Exception {
    public DataElevationModelNotFoundException() : base() { }
    public DataElevationModelNotFoundException(string message) : base(message) { }
}

class DataElevationModelReadException : Exception {
    public DataElevationModelReadException() : base() { }
    public DataElevationModelReadException(string message) : base(message) { }
}
