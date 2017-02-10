# Video Streaming
#### We use USB Ethernet to transmit UDP packet between PC and android phone. PC is the server which keep sending real-time pics after receiving request. Phone is the client which send request with user's touching point coordinates, and show pics received from the server. Specifically, Server part is programmed in Java and client part is programmed by Android Strudio.

## Update on 02/10
Currently, after you run both codes, you can touch a point on the image showed on you phone and server will sent 5 packets with 1024 byte in each packet. Sending and receiving on both parts work successfully. For android part, use tag **"UDP"** to view the client log, and use tag **"DISPLAY"** to view screen resolution in pixels and touched point coordinates.

**Problem**: 
- Server need to read a full pic by byte into a queue, and stop sending when the queue is empty.
- Client can not show the pic.
- The latency is about **9ms**, from the client send request to completely receive **5 packets with 1024 byte**.

**Further work**: Modify server part.
