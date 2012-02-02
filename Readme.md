# dbdeploy.NET

.NET port of [dbdeploy](http://code.google.com/p/dbdeploy/)

Manages the deployment of numbered change scripts to a SQL database, using a simple table in the database to track applied changes.

Read the [getting started guide](http://code.google.com/p/dbdeploy/wiki/GettingStarted) for an introduction to what dbdeploy can do. 

More details can be found at [http://www.dbdeploy.com](http://www.dbdeploy.com).

## State

This repository is a clone of the original dbeploy.NET port on [sourceforge](http://sourceforge.net/projects/dbdeploy-net/) to backport all changes of dbdeploy version 3.

These are mainly:
 
 - Applying directly to the database
 - Script generation based on template files