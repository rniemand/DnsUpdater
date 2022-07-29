# SonarQube

## Requirements

### Java (used by SonarQube)

- Download JDK [from here](https://jdk.java.net/18/)
- Extract it somewhere locally: `C:\Java\jdk-18.0.1.1`
- Add a new **System Environment Variable** called `JAVA_HOME` = `C:\Java\jdk-18.0.1.1`
- Add the following path to the systems `PATH` **Environment Variable** - `C:\Java\jdk-18.0.1.1\bin`
- Open up a console and type `java -version` to ensure that everything is working as expected.

### Docker Desktop

- Download and install Docker Desktop [from here](https://www.docker.com/products/docker-desktop/)
- Ensure that you have WSL enabled and all the other guff needed for Docker to run.
- You will most likely need to restart your computer.

## First Time Setup

- Start the container `docker-compose up`
- Connect to local SQL Server usign `sa`:`saP@ssw0rd`
- Create a new DB called `SonarQube` using the `sql_latin1_general_cp1_ci_as` collation.
  - Use the `Simple` recovery model unless you have a good reason not to use it
- Restart the container
  - `CTRL` + `C` -> `docker-compose down` to stop the container
  - `docker-compose up` to start the container
- Watch the output logs to ensure everything started correctly
- Confirm everything is working by connecting to [SonarQube](http://localhost:9000).

### Creating Global API Key

- Connect to [SonarQube](http://localhost:9000) and set your initial account credentials.
- Navigate to [security and tokens](http://localhost:9000/account/security/) and create a new **Global Analysis Token** making sure to record it's value.
- You can now use this key with all your local applications while developing.

## Project Setup

- Connect to [SonarQube](http://localhost:9000) and create a [new manual project](http://localhost:9000/projects/create?mode=manual).
- Give your project a **name** and a **project key**.
- Click setup to complete the process
- Optionally you can create a new [token](http://localhost:9000/account/security/) for this project or use the global one created earlier.
