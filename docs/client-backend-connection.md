# Client to backend connection implementation

This will be a document outlining the implementation details of the client to backend connection in our application.

1. **Overview**
	The client will take jobs completely from the backend and sync them to local storage, when it cant reach the server it will run the jobs from local else what it gets from the server.
	The client will have to register to the server by not having its UUID(has to be saved somewhere in local storage) and the server will get the request and wait for an admin to approve it,
	when an admin approves the request it will send a responce to the client with a UUID and it will use that UUID to identify itself in future requests.
	There will be healthchecks, that means there will be a health check endpoint and the clients will access that with their UUID and by that the server will know if the client is online or offline,
	the client will acces this endpint the every minute it runs, the server will put it as offline if it doesnt get the healtcheck in 3 minutes.
	The creation of a client will be removed and will be replaced by the registration process, to the server dashboard will be added a section for how many things are online or not.
	The clients will report failed jobs or successful jobs to the server, and the server will have a dashboard to show the status of all clients and their jobs, this will also get written into logs.