# Learn about building .NET container images:
# https://github.com/dotnet/dotnet-docker/blob/main/samples/README.md
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG TARGETARCH
WORKDIR /source

# Copy project file and restore as distinct layers
COPY --link YamamotoOfficerBot/*.csproj YamamotoOfficerBot/
RUN cd ./YamamotoOfficerBot && dotnet restore -a $TARGETARCH

# Copy source code and publish app
COPY --link YamamotoOfficerBot/* YamamotoOfficerBot/
RUN cd ./YamamotoOfficerBot && dotnet publish -a $TARGETARCH --no-restore -o /app


# Runtime stage
FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /app
COPY --link --from=build /app .
ENTRYPOINT ["/app/YamamotoOfficerBot"]