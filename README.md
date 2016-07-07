# CmdlnprintService

Service to create PDF files from a url using firefox and cmdlnprint addon under Linux

This is a tool that runs in a HTTP server to execute [Firefox](https://www.mozilla.org/firefox/) with the [cmdlnprint](https://addons.mozilla.org/es/firefox/addon/cmdlnprint) addon installed to create and download the printing as a PDF file.

```bash
# run with --help
cmdlnprint-service.exe --help
```

This project run under linux, to compile it you will need xbuild (in Debian/GNU Linux the package is mono-xbuild).

You will need a desktop to run firefox, I create a VNC desktop for that task.

To avoid user problems, the server must be instantiated for the same user that the desktop is.

# Create executable

Use the `compile` script, run `./compile` to see the options

```bash
./compile clean
./compile build
# This will put the executable file into /opt/CmdlnprintService/
./compile install
```

# How to test in your machine

- You will need firefox running and with cmdlnprint add-on installed
- Close any Firefox instance and open the file testingpage/testinpage.htm in other different browser (like epiphany)
- On the testing page set the options and submit, you will see an instance of firefox running and will receive the created page. The log will appear on the place you run cmdlnprint-service.exe

# Contributing

This is free software (under the terms of the LICENSE), you are free to contribute, propose ideas, fill bugs or colaborate in any other ways. Please, do not contact me directly, lets use the Github tools.

# Ideas and task to do

- As an idea, it could simply pass all the arguments received in the request, include the `-print-*` prefix with the value and pass the information as is to the command line
- Make a better `testingpage/testinpage.htm` page

# Author and license

This software is created by Carlos C Soto <eclipxe13@gmail.com> and is under the terms of the MIT License, see LICENSE file for more information.

