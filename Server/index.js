const express = require('express');
const app = express();
const http = require('http');
const server = http.createServer(app);
const port = 8888;
const io = require('socket.io')(server);
console.log(`socket up and running @ http://localhost:${port}`);


const defaultRoom = 'Lobby';


io.on('connection', function(client) {
    client.on('new user', (data) => {
        socket.join(defaultRoom);
        io.in(defaultRoom).emit('user joined', data);  //data is role and/or id
    });

    client.on('event', (data) => {
        io.in(defaultRoom).emit('server to client', data);
    });
})


server.listen(port);
console.log(`server up and running @ http://localhost:${port}`);
