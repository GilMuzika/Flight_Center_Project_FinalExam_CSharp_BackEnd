This directory contains the text files used by the projects of the solution and didn't been placed in the respective "bin" directories of each project.
The exact path to each file is placed to the "secret vault", the "secrets.xml" file in the root directory of the solution, each path as a separate "secret".

To use secrets you need also add the secret to the "Web.congig" file in XML syntax as such:
<add key="JWT_SecretKey" value="dummyString" />