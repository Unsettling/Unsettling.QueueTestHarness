# Queue Test Harness

## Introduction

A dotnet-core console application that can be used in a Docker container to pick 
up messages from RabbitMQ.

- The queue is provided by [RabbitMQ][1].
- The client library to RabbitMQ is [EasyNetQ][2].
- [Docker][3] containers isolate the different parts.

## RabbitMQ Dockerized

Run RabbitMQ with the management plugin in a Docker container:

```
$ docker run -d -p 15672:15672 -p 5672:5672 -h subscriber-rabbit --name subscriber-rabbit rabbitmq:3-management
```

## Docker

With [Docker Toolbox][5] there're a few differences.
Set `docker.local` as an entry in a HOSTS file that points to the Docker 
Machine IP \[[1](#dockerlocal)\]. The IP is listed when you run 
`docker-machine env`. Now log in to http://docker.local:15672/ 
(default: guest/guest).

## How To Run The Subscriber

After restoring all the packages and building all the projects the following 
steps will create and run the container for the subscriber:

```
$ dotnet publish .\src\Unsettling.Dequeue\ -c debug
$ docker-compose -f .\docker-compose.development.yml build
$ docker run -d unsettling/dequeue:development
```

Now if you log into your local RabbitMQ you will see activity on a queue when 
you publish a message.

```
$ dotnet .\src\Unsettling.Enqueue\bin\Debug\netcoreapp1.1\Unsettling.Enqueue.dll
```

The queue URIs are hard-coded to keep the code size down. The `docker.local` 
refers to an entry in a HOSTS file that points to the Docker Machine IP but the 
subscriber requires the IP as it doesn't have access to the HOSTS file.

**Note**: Watch out for the case-sensitive Dockerfile not recognising the `Debug` 
directory. You can just rename the folder on the filesystem to `debug`.

## Code Explanation

### Microsoft.AspNetCore.Hosting

Traditionally a console application keep-alive could be managed with a single 
`Console.Readline()` but this won't work with dotnet-core in a Windows Service, 
where the original Windows-centric APIs are no longer available, or a Docker 
container, where the STDIN redirection wouldn't work.

The answer is to host the console application. An easily accessible solution 
lies in using the `ASP.NET Core Host`. We set this up in our application's 
`Main()` method and it builds whatever is defined in the `Startup` class. 
Now `host.Run()` will run the `Start()` method from our implementation of 
`IServer` (in the `Application` class).

### Startup

**Startup.cs** does the normal configuration of the application that you'd 
expect in a dotnet core application. In this example code I've foregone config 
to minimise the code you have to read.

### Application

`IServer` defines a `Start` method in **Application.cs**. It's setting up 
a consumer for a single message type on a named queue in [RabbitMQ][1]. A 
handler is defined: a delegate that will do something when that message is put 
on the queue. The code here is massively simplified by the syntatic sugar of 
[EasyNetQ][2] which provides an easy .NET API for [RabbitMQ][1].

## References

<a name="dockerlocal"></a> \[1\] [Docker for Windows: Communication between linux and windows containers][4]

[1]: https://www.rabbitmq.com/ "RabbitMQ"
[2]: http://easynetq.com/ "EasyNetQ"
[3]: https://www.docker.com/ "Docker"
[4]: https://stackoverflow.com/a/45577713/444244 "Docker for Windows: Communication between linux and windows containers"
[5]: https://www.docker.com/products/docker-toolbox "Docker Toolbox"