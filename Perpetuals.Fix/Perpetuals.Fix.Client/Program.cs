using QuickFix.Logger;
using QuickFix.Store;
using QuickFix.Transport;
using QuickFix;
var app = new Perpetuals.Fix.Client.FixClientApp();
var settings = new SessionSettings("client.cfg");
var storeFactory = new FileStoreFactory(settings);
var logFactory = new FileLogFactory(settings);
var initiator = new SocketInitiator(app, storeFactory, settings, logFactory);

app.MyInitiator = initiator;
initiator.Start();

app.Run(); 

initiator.Stop();