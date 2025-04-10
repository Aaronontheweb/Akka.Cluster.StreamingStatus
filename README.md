# Akka.Cluster.StreamingStatus

The `Akka.Cluster.StreamingStatus` project is designed to demonstrate the use of Akka.NET's clustering capabilities. It provides a scalable and resilient system for streaming status updates across a distributed network of nodes. This project leverages Akka.Hosting for configuration and management, making it easier to deploy and manage Akka.NET applications.

## Building and Running the Docker Image Locally

To build the Docker image using the .NET SDK's built-in Docker support, execute the following command:

```bash
dotnet publish --os linux --arch x64 /t:PublishContainer
```

This command will create a Docker image named `akka.cluster.streamingstatus:latest`.

## Launching the Docker Compose Script

To run the application using Docker Compose, use the following command:

```bash
docker-compose up
```

This will start the services defined in the `docker-compose.yml` file, including the `mainnode` and `streaming` services.

## Scaling the `streaming` Service

To scale the `streaming` service, you can use the `docker-compose` command to increase the number of instances. For example, to scale the `streaming` service to 3 instances, run:

```bash
docker-compose up --scale streaming=3
```

This will create additional instances of the `streaming` service, allowing you to test the scalability and resilience of the Akka.NET cluster.

## Interacting with the Cluster using `pbm`

For an interactive experience with the cluster, consider installing `pbm` (Petabridge.Cmd), which allows you to send commands to the cluster. The `mainnode` in the Docker Compose setup runs on `pbm`'s default listening port, making it easy to connect and interact with the cluster.

To install `pbm`, follow the instructions at [Petabridge.Cmd](https://cmd.petabridge.com).

## Additional Information

For more details on the commands and configurations used in this project, refer to the `docker-compose.yml` and other configuration files included in the repository.

## Learn More

Want to learn Akka.NET? Try Akka.NET Bootcamp: [https://petabridge.com/bootcamp](https://petabridge.com/bootcamp)

Enjoy exploring the capabilities of Akka.NET clustering with this project!