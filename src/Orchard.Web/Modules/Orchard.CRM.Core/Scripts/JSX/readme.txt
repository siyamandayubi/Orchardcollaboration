/// Orchard Collaboration is a series of plugins for Orchard CMS that provides an integrated ticketing system and collaboration framework on top of it.
/// Copyright (C) 2014-2016  Siyamand Ayubi
///
/// This file is part of Orchard Collaboration.
///
///    Orchard Collaboration is free software: you can redistribute it and/or modify
///    it under the terms of the GNU General Public License as published by
///    the Free Software Foundation, either version 3 of the License, or
///    (at your option) any later version.
///
///    Orchard Collaboration is distributed in the hope that it will be useful,
///    but WITHOUT ANY WARRANTY; without even the implied warranty of
///    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
///    GNU General Public License for more details.
///
///    You should have received a copy of the GNU General Public License
///    along with Orchard Collaboration.  If not, see <http://www.gnu.org/licenses/>.

The js files have been generated from JSX files. The steps to prepare the environment is as follows

1) Download andd install Nodejs. The address of the website is: https://nodejs.org/en/download/
2) Make sure that the installer installss npm too.
3) Follow the steps recommended in: https://facebook.github.io/react/docs/tooling-integration.html
   In summary:
   a) Run the following commands in the command prompt in Admin mode.
	  npm install -g babel-cli
      npm install babel-preset-es2015 babel-preset-react
   b) Navigate to the Orchard.CRM.Core in windows explorer, open a command prompt there and run the following command
     babel --presets react --watch scripts/jsx --out-dir scripts



	 /// Take a look at http://sawyerhollenshead.com/writing/using-jquery-ui-sortable-with-react/ for combining JQueryUI and Reactjs
