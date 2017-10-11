Yggdrasil
=============================================================================

Future base library for the Aura Project's game servers. *Work in progress.*

This basically moves current Shared classes to a dedicated library.

Libraries
-----------------------------------------------------------------------------

### Yggdrasil

Commonly needed classes, such as TCP client and server, logger,
file reader, hashing, and other utility classes.

### Yggdrasil.Ai

Base classes for AI development.

### Yggdrasil.Data
*Depends on: Yggdrasil*

Base classes for reading databases from text files.

### Yggdrasil.Data.JSON
*Depends on: Yggdrasil.Data*

Extension for Yggdrasil.Data, for JSON support.

### Yggdrasil.Structures

Structures, such as `QuadTree`.

### Yggdrasil.Network.WebSocket
*Depends on: Yggdrasil*

Extension for Yggdrasil.Network, adding a WebSocketConnection
to accept connections from WebSockets, and other related classes.

Links
-----------------------------------------------------------------------------
* GitHub: https://github.com/aura-project/Yggdrasil

Build Status
-----------------------------------------------------------------------------
[![Build Status](https://travis-ci.org/aura-project/Yggdrasil.png?branch=master)](https://travis-ci.org/aura-project/Yggdrasil)
