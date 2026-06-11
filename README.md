# Jeremy Serialization Library (JSL)

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![Framework: .NET 8.0](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/download/dotnet/8.0)

JSL (Jeremy Serialization Library) is a high-performance, unaligned bit-level serialization and zero-allocation object-pooling framework written in C#. It is specifically optimized for low-latency multiplayer games and networking applications where bandwidth usage and Garbage Collection (GC) overhead must be minimized.

---

## Key Features

* **Unaligned Bit-Level Packing**: Write and read data down to individual bits (e.g., writing a 7-bit integer or a 1-bit boolean flag) using custom `BitWriter` and `BitReader` implementations.
* **Zero-Allocation Object Pooling**: Reference-counted, type-safe pooling (`NetRecyclable`) for streams, messages, and lists (`NetList<T>`) to eliminate runtime heap allocations and garbage collection overhead.
* **Optimized Network Types**:
  * **`NetVector2`**: Quantized 2D vector coordinate serialization using precisely 18 bits per coordinate.
  * **`NetCompressedVector3`**: Compressed 3D coordinates (18-bit X/Y, 14-bit Z).
  * **`NetQuat`**: Quantized Quaternion packing using the "largest component drop" method to compress full 3D rotations into exactly 32 bits.
  * **`NetUnitFloat`**: Compresses floats bounded in the `[-1.0, 1.0]` range into 16-bit integers.
* **No-Reflection Code Generation**: Support for statically generated routing factories that deserialize messages based on type IDs without using slow reflection.
* **Thread Safety**: Uses thread-local resources where appropriate (e.g., zero-allocation string builders during deserialization) to support safe concurrent executions.

---

## Directory Structure

```text
├── JSL/                  # Core library containing buffers, pooling, networking types, and utility classes
├── JSL.CodeGen/          # Static code generator for serializable messages and type dispatchers
├── JSLTest/              # Test suite (22 unit tests) organized by component
├── LICENSE               # MIT License file
└── README.md             # Project documentation
```

---

## Quick Start

### Basic Serialization

```csharp
using JSL.Buffers;
using JSL.Pools;

// 1. Acquire a write stream from the pool
using (var stream = MemoryManager.Instance.WriteStreamPool.Get())
{
    // Write native types
    stream.Write((byte)250);
    stream.Write(12345678);
    stream.Write(123.456f);
    
    // Write custom-bit-width variables
    stream.WriteBits(42, 6); // Writes the value 42 using exactly 6 bits
    stream.WriteIntRange(15, 10, 20); // Quantizes a value between 10 and 20

    // 2. Extract serialized bytes
    var buffer = new byte[256];
    var size = stream.CopyBytes(buffer);

    // 3. Fill a read stream to deserialize
    using (var reader = MemoryManager.Instance.ReadStreamPool.Get())
    {
        reader.Fill(buffer, 0, size);

        byte b = reader.ReadByte();
        int i = reader.ReadInt32();
        float f = reader.ReadSingle();
        uint customBits = reader.ReadBits(6);
        int ranged = reader.ReadIntRange(10, 20);
    }
}
```

### Pooling and Reference Counting

JSL uses a reference-counting mechanism (`Acquire()` / `Dispose()`) to manage object lifetimes in the pool:

```csharp
// Acquire an instance from the pool (ref count starts at 1)
var message = MemoryManager.RecyclablePool.Get<NetMessage>();

// Share ownership (increments ref count)
message.Acquire(); 

// Dispose decrements the reference count.
// Once it reaches 0, the object is automatically returned to the pool.
message.Dispose(); // Ref count goes back to 1
message.Dispose(); // Ref count goes to 0 (recycled)
```

---

## Running Tests

 JSL contains a comprehensive test suite covering serialization correctness, memory safety, pool allocations, and numeric compression. To run the tests, execute:

```bash
dotnet test
```

---

## License

This project is licensed under the [MIT License](LICENSE) - see the LICENSE file for details.

Copyright (c) 2026 Jeremy Anderson (github: jbluepolarbear, email: jbluepolarbear@gmail.com, website: jeremyrobertanderson.com)
