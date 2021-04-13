### Trezor Balance Monitoring
###### This application can run full time and serve as a near-realtime look into crypto balances. It's based off of the Trezor line of devices in that it's using the trezor firmware repository to get a list of coins, but any address for that specific coin will work. It doesn't need to be a trezor specific address.
###### Each coin requires a RPC connection. I run a node for each coin I'm monitoring within my own network, but public RPC servers should work as well
![Alt text](/TrezorBalanceMonitoring/Resources/TrezorMonitoring.PNG?raw=true "Optional Title")

#### Configuration
Update the exe.config file for your own Coin Market Cap API Key and also for coin address and RPC endpoints. A sample config for SysCoin would look like this:
~~~
<add key="SYS" coinId="SYS" coinName="Syscoin" address="sys1q6q69qvdp5xpn4pmunhk4clswlmp6zw8hgun7xv" rpcendpoint="http://192.168.1.36:9001" rpcuser="api" rpcpassword="api" />
~~~
