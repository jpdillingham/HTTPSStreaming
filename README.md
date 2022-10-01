# HTTPSStreaming
A proof of concept for concurrent file streaming over HTTPS

## Purpose

This repository is a proof of concept for a cloud/on-prem hybrid file transfer system.  This system consists of two parts:

* A `Server` application running in the cloud somewhere.  It has full networking capabilities (can receive incoming connections to any port)
* A `Client` application running on-prem (like someone's house or dorm room).  It can create outbound connections on any port, but can't receive incoming connections at all, due to NAT or firewall(s) outside of the control of the user.

We want to serve files hosted on the client machine to the internet, but because the client machine can't accept incoming connections, this is impossible.

![image](https://user-images.githubusercontent.com/17145758/193414094-df5c920c-9c8a-422a-b46d-fb77d87fe385.png)

## Connections

The `Client` establishes an outbound connection to the `Server` using the [SignalR Protocol](https://github.com/dotnet/aspnetcore/blob/main/src/SignalR/docs/specs/HubProtocol.md) (tl;dr, a websocket).  This connection is used as a control/command connection, and it remains active indefinitely so that the server can send unsolicited messages to the client.

The `Client` sends requested data back to the `Server` over standard HTTP/S.

## Sequence

The `Server`, upon receiving an external request for a file hosted on the `Client` machine, sends a request for the file over the control/command connection.

The `Client` sends the contents of the requested file to the `Server` via a multipart HTTP request.

The `Server` caches the contents of the file locally, then sends the contents to the requestor.

## Caveats

The server has no idea which files the client is hosting so it can't produce a list for exernal clients, and instead those clients must blindly request files that they've learned exist through other means.

Upon startup, the client should enumerate available files, and send that list to the server either over the control/command connection, or over HTTP/S.  The server can then advertise this list to external callers.

The list will need to be updated periodically, to ensure the server isn't advertising files that have moved, been renamed, or are no longer available.
