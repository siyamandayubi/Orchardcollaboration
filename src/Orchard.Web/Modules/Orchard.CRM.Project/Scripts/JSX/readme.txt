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
