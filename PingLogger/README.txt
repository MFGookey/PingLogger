This application will ping the Google DNS servers located at IPv4 address 8.8.8.8 once every ten seconds.
If any of the ping responses are not successful, an entry will be made in the logfile and the pings will be sent once every second until the next successful response.

Each execution of the program has it's own logfile, located at C:\pinglogs\ with the filename set to the date and time at which the program began executing.