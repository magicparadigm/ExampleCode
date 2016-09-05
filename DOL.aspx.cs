using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Configuration;
using System.Net;
using System.IO;

//using ServiceReference1;
using System.Collections;
using Newtonsoft.Json;

using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Xml;
using System.Net.Http;

public partial class demos_DOL : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            AccountInfo.Visible = true;
            primarySignerSection.Visible = true;
            jointSignerSection.Visible = false;
            templates.Visible = false;
            SupplementalDocConfig.Visible = false;
            button.Visible = true;
            button2.Visible = false;
            button3.Visible = false;
            uploadfile2validator.Visible = false;
            uploadfilevalidator.Visible = false;
            tabPageValidator.Visible = false;
            tabNamevalidator.Visible = false;
            xPositionValidator.Visible = false;
            yPositionValidator.Visible = false;
            
            UploadButton.InnerText = "Upload";
            UploadButton2.InnerText = "Upload";
            button.InnerText = "Next";
            button2.InnerText = "Next";
            button3.InnerText = "Submit";
            docusignFrame.Visible = false;
            docusignFrameIE.Visible = false;
        }

        // Add event handlers for the navigation button on each of the wizard pages 
        PrefillClick.ServerClick += new EventHandler(prefill_Click);
        button.ServerClick += new EventHandler(button_Click);
        button2.ServerClick += new EventHandler(button2_Click);
        button3.ServerClick += new EventHandler(button3_Click);
        UploadButton.ServerClick += new EventHandler(uploadButton_Click);
        UploadButton2.ServerClick += new EventHandler(uploadButton2_Click);
    }

    protected void prefill_Click(object sender, EventArgs e)
    {
        firstname.Value = "Warren";
        lastname.Value = "Buffet";
        email.Value = "magicparadigm@live.com";
        tabName.Value = "PrimarySignerSignature";
        tabPage.Value = "1";
        xPosition.Value = "175";
        yPosition.Value = "315";
    }

    protected void button_Click(object sender, EventArgs e)
    {
        if (!email.Value.Equals("") && !firstname.Value.Equals("") && !lastname.Value.Equals(""))
        {
            AccountInfo.Visible = false;
            primarySignerSection.Visible = false;
            jointSignerSection.Visible = false;
            templates.Visible = true;
            SupplementalDocConfig.Visible = false;
            button.Visible = false;
            button2.Visible = true;
            uploadfile2validator.Visible = true;
            uploadfilevalidator.Visible = true;
            tabPageValidator.Visible = true;
            tabNamevalidator.Visible = true;
            xPositionValidator.Visible = true;
            yPositionValidator.Visible = true;
        }
    }

    protected void button2_Click(object sender, EventArgs e)
    {
        if (!uploadFile.Value.Equals("") && !uploadFile2.Value.Equals("") && !tabPage.Value.Equals("") && !tabName.Value.Equals("") && !xPosition.Value.Equals("") && !yPosition.Value.Equals(""))
        {
            AccountInfo.Visible = false;
            primarySignerSection.Visible = false;
            jointSignerSection.Visible = false;
            templates.Visible = false;
            SupplementalDocConfig.Visible = true;
            button2.Visible = false;
            button3.Visible = true;
        }
    }

    protected void button3_Click(object sender, EventArgs e)
    {
        AccountInfo.Visible = false;
        primarySignerSection.Visible = false;
        jointSignerSection.Visible = false;
        templates.Visible = false;
        SupplementalDocConfig.Visible = false;
        mainForm.Visible = false;
        button3.Visible = false;
        createEnvelope();

    }


    protected void uploadButton_Click(object sender, EventArgs e)
    {
        try
        {
            if (FileUpload1.HasFile)
            {
                String filename = Path.GetFileName(FileUpload1.FileName);
                FileUpload1.SaveAs(Server.MapPath("~/App_Data/") + filename);
                uploadFile.Value = filename;
            }
        }
        catch (Exception ex)
        {
            uploadFile.Value = "Upload status: The file could not be uploaded. The following error occured: " + ex.Message;
        }
    }

    protected void uploadButton2_Click(object sender, EventArgs e)
    {
        try
        {
            if (FileUpload2.HasFile)
            {
                String filename = Path.GetFileName(FileUpload2.FileName);
                FileUpload2.SaveAs(Server.MapPath("~/App_Data/") + filename);
                uploadFile2.Value = filename;
            }
        }
        catch (Exception ex)
        {
            uploadFile2.Value = "Upload status: The file could not be uploaded. The following error occured: " + ex.Message;
        }
    }


    protected String RandomizeClientUserID()
    {
        Random random = new Random();

        return (random.Next()).ToString();
    }


    public class Document
    {
        public string name { get; set; }
        public string documentId { get; set; }
        public string display { get; set; }
        public Boolean includeInDownload { get; set; }
        public string signerMustAcknowledge { get; set; }
        public string fileExtension { get; set; }
    }

    public class SignHereTab
    {
        public string tabId { get; set; }
        public string name { get; set; }
        public string pageNumber { get; set; }
        public string documentId { get; set; }
        public string yPosition { get; set; }
        public string xPosition { get; set; }
    }

    public class Tabs
    {
        public List<SignHereTab> signHereTabs { get; set; }
    }

    public class Signer
    {
        public Tabs tabs { get; set; }
        public string routingOrder { get; set; }
        public string email { get; set; }
        public string name { get; set; }
        public string recipientId { get; set; }
        public string roleName { get; set; }
        public string clientUserId { get; set; }
    }

    public class Recipients
    {
        public List<Signer> signers { get; set; }
    }

    public class CreateEnvelopeRequest
    {
        public string status { get; set; }
        public string emailBlurb { get; set; }
        public string emailSubject { get; set; }
        public List<Document> documents { get; set; }
        public Recipients recipients { get; set; }
    }

    private static void WriteStream(Stream reqStream, string str)
    {
        byte[] reqBytes = UTF8Encoding.UTF8.GetBytes(str);
        reqStream.Write(reqBytes, 0, reqBytes.Length);
    }

    private String GetSecurityHeader()
    {
        String str = "";
        if ((acctEmail.Value.Length == 0) && (password.Value.Length == 0) && (integratorKey.Value.Length == 0)) 
        {
            str = "<DocuSignCredentials>" + "<Username>" + ConfigurationManager.AppSettings["API.Email"] + "</Username>" +
                "<Password>" + ConfigurationManager.AppSettings["API.Password"] + "</Password>" +
                "<IntegratorKey>" + ConfigurationManager.AppSettings["API.IntegratorKey"] + "</IntegratorKey>" +
                "</DocuSignCredentials>";
        }
        else
        {
            str = "<DocuSignCredentials>" + "<Username>" + acctEmail.Value + "</Username>" +
                "<Password>" + password.Value  + "</Password>" +
                "<IntegratorKey>" + integratorKey.Value + "</IntegratorKey>" +
                "</DocuSignCredentials>";
        }
        return str;
    }

    public class CreateEnvelopeResponse
    {
        public string envelopeId { get; set; }
        public string uri { get; set; }
        public string statusDateTime { get; set; }
        public string status { get; set; }
    }
    public class RecipientViewRequest
    {
        public string authenticationMethod { get; set; }
        public string email { get; set; }
        public string returnUrl { get; set; }
        public string userName { get; set; }
        public string clientUserId { get; set; }
    }

    public class RecipientViewResponse
    {
        public string url { get; set; }
    }



    protected void createEnvelope()
    {

       
        // Set up the envelope
        CreateEnvelopeRequest createEnvelopeRequest = new CreateEnvelopeRequest();
        createEnvelopeRequest.emailSubject = "DOL Example";
        createEnvelopeRequest.status = "sent";
        createEnvelopeRequest.emailBlurb = "Example of how DOL functionality could work";

        // Add documents (one main document and one supplemental document to show as terms and conditions)
        createEnvelopeRequest.documents = new List<Document>();
        Document mainDocument = new Document();
        mainDocument.name = "Main Document";
        mainDocument.documentId = "1";
        mainDocument.fileExtension = "doc";
        createEnvelopeRequest.documents.Add(mainDocument);

        Document supplementalDocument = new Document();
        supplementalDocument.name = "Terms & Conditions";
        supplementalDocument.documentId = "2";
        supplementalDocument.fileExtension = "doc";

        if (IncludeInDownloadTrue.Checked)
            supplementalDocument.includeInDownload = true;
        else if (IncludeInDownloadFalse.Checked)
            supplementalDocument.includeInDownload = false;

        // Registration type
        if (Modal.Checked)
        {
            supplementalDocument.display = "modal";
        }
        else if (Download.Checked)
        {
            supplementalDocument.display = "download";    
        }
        else if (Inline.Checked)
        {
            supplementalDocument.display = "inline";
        }

        if (View_.Checked)
        {
            supplementalDocument.signerMustAcknowledge = "view";
        }
        else if (Accept.Checked)
        {
            supplementalDocument.signerMustAcknowledge = "accept";
        }
        else if (View_Accept.Checked)
        {
            supplementalDocument.signerMustAcknowledge = "view_accept";
        }
        else if (No_Interaction.Checked)
        {
            supplementalDocument.signerMustAcknowledge = "no_interaction";
        }

        createEnvelopeRequest.documents.Add(supplementalDocument);

        // Add recipients 
        createEnvelopeRequest.recipients = new Recipients();
        createEnvelopeRequest.recipients.signers = new List<Signer>();
        Signer signerOne = new Signer();
        signerOne.name = firstname.Value + " " + lastname.Value;
        signerOne.email = email.Value;
        signerOne.recipientId = "1";
        signerOne.roleName = "signer1";
        signerOne.routingOrder = "1";
        signerOne.clientUserId = RandomizeClientUserID();

        // Add tab for the recipient
        signerOne.tabs = new Tabs();
        signerOne.tabs.signHereTabs = new List<SignHereTab>();
        SignHereTab signHereTab = new SignHereTab();
        signHereTab.documentId = "1";
        signHereTab.pageNumber = tabPage.Value;
        signHereTab.tabId = "1";
        signHereTab.xPosition = xPosition.Value;
        signHereTab.yPosition = yPosition.Value;
        signHereTab.name = "sigTab";
        signerOne.tabs.signHereTabs.Add(signHereTab);

        createEnvelopeRequest.recipients.signers.Add(signerOne);

        string output = JsonConvert.SerializeObject(createEnvelopeRequest);

        if (accountId.Value.Length == 0)
            accountId.Value = ConfigurationManager.AppSettings["API.AccountID"];
        
        // Specify a unique boundary string that doesn't appear in the json or document bytes.
        string Boundary = "MY_BOUNDARY";

        // Set the URI
        HttpWebRequest request = HttpWebRequest.Create(ConfigurationManager.AppSettings["DocuSignServer"] + "/restapi/v2/accounts/" + accountId.Value + "/envelopes") as HttpWebRequest;

        // Set the method
        request.Method = "POST";

        // Set the authentication header
        request.Headers["X-DocuSign-Authentication"] = GetSecurityHeader();

        // Set the overall request content type aand boundary string
        request.ContentType = "multipart/form-data; boundary=" + Boundary;
        request.Accept = "application/json";

        // Start forming the body of the request
        Stream reqStream = request.GetRequestStream();

        // write boundary marker between parts
        WriteStream(reqStream, "\n--" + Boundary + "\n");

        // write out the json envelope definition part
        WriteStream(reqStream, "Content-Type: application/json\n");
        WriteStream(reqStream, "Content-Disposition: form-data\n");
        WriteStream(reqStream, "\n"); // requires an empty line between the header and the json body
        WriteStream(reqStream, output);

        // write out the form bytes for the first form
        WriteStream(reqStream, "\n--" + Boundary + "\n");

        WriteStream(reqStream, "Content-Type: application/pdf\n");
        WriteStream(reqStream, "Content-Disposition: file; filename=\"Main Document\"; documentId=1\n");
        WriteStream(reqStream, "\n");
        String filename = uploadFile.Value;
        if (File.Exists(Server.MapPath("~/App_Data/" + filename)))
        {
            // Read the file contents and write them to the request stream
            byte[] buf = new byte[4096];
            int len;
            // read contents of document into the request stream
            FileStream fileStream = File.OpenRead(Server.MapPath("~/App_Data/" + filename));
            while ((len = fileStream.Read(buf, 0, 4096)) > 0)
            {
                reqStream.Write(buf, 0, len);
            }
            fileStream.Close();
        }

        // write out the form bytes for the second form
        WriteStream(reqStream, "\n--" + Boundary + "\n");

        WriteStream(reqStream, "Content-Type: application/pdf\n");
        WriteStream(reqStream, "Content-Disposition: file; filename=\"Supplemental Document\"; documentId=2\n");
        WriteStream(reqStream, "\n");
        String filename2 = uploadFile2.Value;
        if (File.Exists(Server.MapPath("~/App_Data/" + filename2)))
        {
            // Read the file contents and write them to the request stream
            byte[] buf = new byte[4096];
            int len;
            // read contents of document into the request stream
            FileStream fileStream = File.OpenRead(Server.MapPath("~/App_Data/" + filename2));
            while ((len = fileStream.Read(buf, 0, 4096)) > 0)
            {
                reqStream.Write(buf, 0, len);
            }
            fileStream.Close();
        }
        // wrte the end boundary marker - ensure that it is on its own line
        WriteStream(reqStream, "\n--" + Boundary + "--");
        WriteStream(reqStream, "\n");

        try
        {
            HttpWebResponse response = request.GetResponse() as HttpWebResponse;

            if (response.StatusCode == HttpStatusCode.Created)
            {
                byte[] responseBytes = new byte[response.ContentLength];
                using (var reader = new System.IO.BinaryReader(response.GetResponseStream()))
                {
                    reader.Read(responseBytes, 0, responseBytes.Length);
                }
                string responseText = Encoding.UTF8.GetString(responseBytes);
                CreateEnvelopeResponse createEnvelopeResponse = new CreateEnvelopeResponse();

                createEnvelopeResponse = JsonConvert.DeserializeObject<CreateEnvelopeResponse>(responseText);
                if (createEnvelopeResponse.status.Equals("sent"))
                {
                    // Now that we have created the envelope, get the recipient token for the first signer
                    String url = Request.Url.AbsoluteUri;
                    RecipientViewRequest recipientViewRequest = new RecipientViewRequest();
                    recipientViewRequest.authenticationMethod = "email";
                    recipientViewRequest.clientUserId = signerOne.clientUserId;
                    recipientViewRequest.email = email.Value;
                    if (!Request.Browser.IsMobileDevice)
                    {
                        recipientViewRequest.returnUrl = url.Substring(0, url.LastIndexOf("/")) + "/EmbeddedSigningComplete0.aspx?envelopeID=" + createEnvelopeResponse.envelopeId;
                    }
                    else
                    {
                        recipientViewRequest.returnUrl = url.Substring(0, url.LastIndexOf("/")) + "/ConfirmationPage.aspx?envelopeID=" + createEnvelopeResponse.envelopeId;

                    }
                    recipientViewRequest.userName = signerOne.name;

                    HttpWebRequest request2 = HttpWebRequest.Create(ConfigurationManager.AppSettings["DocuSignServer"] + "/restapi/v2/accounts/" + accountId.Value + "/envelopes/" + createEnvelopeResponse.envelopeId + "/views/recipient") as HttpWebRequest;
                    request2.Method = "POST";

                    // Set the authenticationheader
                    request2.Headers["X-DocuSign-Authentication"] = GetSecurityHeader();

                    request2.Accept = "application/json";
                    request2.ContentType = "application/json";

                    Stream reqStream2 = request2.GetRequestStream();

                    WriteStream(reqStream2, JsonConvert.SerializeObject(recipientViewRequest));
                    HttpWebResponse response2 = request2.GetResponse() as HttpWebResponse;

                    responseBytes = new byte[response2.ContentLength];
                    using (var reader = new System.IO.BinaryReader(response2.GetResponseStream()))
                    {
                        reader.Read(responseBytes, 0, responseBytes.Length);
                    }
                    string response2Text = Encoding.UTF8.GetString(responseBytes);

                    RecipientViewResponse recipientViewResponse = new RecipientViewResponse();
                    recipientViewResponse = JsonConvert.DeserializeObject<RecipientViewResponse>(response2Text);
                    Session.Add("envelopeID", createEnvelopeResponse.envelopeId);

                    // If it's a non-touch aware device, show the signing session in an iFrame
                    if (!Request.Browser.IsMobileDevice)
                    {
                        // If it's a non-touch aware device, show the signing session in an iFrame
                        if (!Request.Browser.Browser.Equals("InternetExplorer") && (!Request.Browser.Browser.Equals("Safari")))
                        {
                            docusignFrame.Visible = true;
                            docusignFrame.Src = recipientViewResponse.url;
                        }
                        else // Handle IE differently since it does not allow dynamic setting of the iFrame width and height
                        {
                            docusignFrameIE.Visible = true;
                            docusignFrameIE.Src = recipientViewResponse.url;
                        }
                    }
                    // For touch aware devices, show the signing session in main browser window
                    else
                    {
                        Response.Redirect(recipientViewResponse.url);
                    }

                }
            }
        }
        catch (WebException ex)
        {
            if (ex.Status == WebExceptionStatus.ProtocolError)
            {
                HttpWebResponse response = (HttpWebResponse)ex.Response;
                using (var reader = new System.IO.StreamReader(ex.Response.GetResponseStream(), UTF8Encoding.UTF8))
                {
                    string errorMess = reader.ReadToEnd();
                    log4net.ILog logger = log4net.LogManager.GetLogger(typeof(demos_DOL));
                    logger.Info("\n----------------------------------------\n");
                    logger.Error("DocuSign Error: " + errorMess);
                    logger.Error(ex.StackTrace);
                    Response.Write(ex.Message);
                }
            }
            else
            {
                log4net.ILog logger = log4net.LogManager.GetLogger(typeof(demos_DOL));
                logger.Info("\n----------------------------------------\n");
                logger.Error("WebRequest Error: " + ex.Message);
                logger.Error(ex.StackTrace);
                Response.Write(ex.Message);
            }
        }

    }
    //    FileStream fs = null;

    //    try
    //    {
    //        String userName = ConfigurationManager.AppSettings["API.Email"];
    //        String password = ConfigurationManager.AppSettings["API.Password"];
    //        String integratorKey = ConfigurationManager.AppSettings["API.IntegratorKey"];


    //        String auth = "<DocuSignCredentials><Username>" + userName
    //            + "</Username><Password>" + password
    //            + "</Password><IntegratorKey>" + integratorKey
    //            + "</IntegratorKey></DocuSignCredentials>";
    //        ServiceReference1.DSAPIServiceSoapClient client = new ServiceReference1.DSAPIServiceSoapClient();

    //        using (OperationContextScope scope = new System.ServiceModel.OperationContextScope(client.InnerChannel))
    //        {
    //            HttpRequestMessageProperty httpRequestProperty = new HttpRequestMessageProperty();
    //            httpRequestProperty.Headers.Add("X-DocuSign-Authentication", auth);
    //            OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = httpRequestProperty;

    //            CompositeTemplate template = new CompositeTemplate();

    //            // Set up the envelope
    //            EnvelopeInformation envInfo = new EnvelopeInformation();
    //            envInfo.AutoNavigation = true;
    //            envInfo.AccountId = ConfigurationManager.AppSettings["API.AccountId"];
    //            envInfo.Subject = "Envelope Custom & Document Fields Example";

    //            // Set up envelope custom fields 
    //            envInfo.CustomFields = new CustomField[envelopeCustomFieldsList.Items.Count];
    //            for (Int16 i = 0; i < envelopeCustomFieldsList.Items.Count; i++)
    //            {
    //                envInfo.CustomFields[i] = new CustomField();
    //                envInfo.CustomFields[i].Name = envelopeCustomFieldsList.Items[i].Text;
    //                envInfo.CustomFields[i].Value = envelopeCustomFieldsList.Items[i].Value;
    //            }

    //            // Set up recipients 
    //            Recipient[] recipients;
    //            if (jointEmail.Value.Trim().Equals(""))
    //            {
    //                recipients = new Recipient[1];
    //            }
    //            else
    //            {
    //                recipients = new Recipient[2];
    //            }

    //            recipients[0] = new Recipient();
    //            recipients[0].ID = "1";
    //            recipients[0].Email = email.Value;
    //            recipients[0].Type = RecipientTypeCode.Signer;
    //            recipients[0].UserName = firstname.Value + " " + lastname.Value;
    //            recipients[0].CaptiveInfo = new RecipientCaptiveInfo();

    //            recipients[0].CaptiveInfo.ClientUserId = RandomizeClientUserID();
    //            recipients[0].RoutingOrder = 1;
    //            recipients[0].RoleName = "Signer1";

    //            // If there is a 2nd recipient, configure 
    //            if (!jointEmail.Value.Equals(""))
    //            {
    //                recipients[1] = new Recipient();
    //                recipients[1].ID = "2";
    //                recipients[1].Email = jointEmail.Value;
    //                recipients[1].Type = RecipientTypeCode.Signer;
    //                recipients[1].UserName = jointFirstname.Value + " " + jointLastname.Value;
    //                recipients[1].RoleName = "Signer2";
    //                recipients[1].RoutingOrder = 1;
    //            }

    //            //Configure the inline templates 
    //            InlineTemplate inlineTemplate = new InlineTemplate();
    //            inlineTemplate.Sequence = "1";
    //            inlineTemplate.Envelope = new Envelope();
    //            inlineTemplate.Envelope.Recipients = recipients;
    //            inlineTemplate.Envelope.AccountId = ConfigurationManager.AppSettings["API.AccountId"];

    //            // Initialize tab properties 
    //            Tab tab = new Tab();
    //            tab.Type = TabTypeCode.SignHere;
    //            tab.XPosition = xPosition.Value;
    //            tab.YPosition = yPosition.Value;
    //            tab.TabLabel = tabName.Value;
    //            tab.RecipientID = "1";
    //            tab.DocumentID = "1";
    //            tab.Name = tabName.Value;
    //            tab.PageNumber = tabPage.Value;

    //            inlineTemplate.Envelope.Tabs = new Tab[] { tab };

    //            template.InlineTemplates = new InlineTemplate[] { inlineTemplate };


    //            // Configure the document
    //            template.Document = new Document();
    //            template.Document.ID = "1";
    //            template.Document.Name = "Sample Document";
    //            BinaryReader binReader = null;
    //            String filename = uploadFile.Value;
    //            if (File.Exists(Server.MapPath("~/App_Data/" + filename)))
    //            {
    //                fs = new FileStream(Server.MapPath("~/App_Data/" + filename), FileMode.Open);
    //                binReader = new BinaryReader(fs);
    //            }
    //            byte[] PDF = binReader.ReadBytes(System.Convert.ToInt32(fs.Length));
    //            template.Document.PDFBytes = PDF;

    //            template.Document.TransformPdfFields = true;
    //            template.Document.FileExtension = "pdf";

    //            // Add document fields
    //            template.Document.DocumentFields = new DocumentField[documentFieldsList.Items.Count];
    //            for (Int16 i = 0; i < documentFieldsList.Items.Count; i++)
    //            {
    //                template.Document.DocumentFields[i] = new DocumentField();
    //                template.Document.DocumentFields[i].Name = documentFieldsList.Items[i].Text;
    //                template.Document.DocumentFields[i].Value = documentFieldsList.Items[i].Value;
    //            }

    //            // Add Connect configuration
    //            envInfo.EventNotification = new EventNotification(); ;
    //            envInfo.EventNotification.URL = ConfigurationManager.AppSettings["ConnectListener"];

    //            envInfo.EventNotification.RequireAcknowledgment = true;
    //            envInfo.EventNotification.RequireAcknowledgmentSpecified = true;
    //            envInfo.EventNotification.LoggingEnabled = true;
    //            envInfo.EventNotification.LoggingEnabledSpecified = true;
    //            envInfo.EventNotification.IncludeTimeZone = true;
    //            envInfo.EventNotification.IncludeTimeZoneSpecified = true;
    //            envInfo.EventNotification.IncludeSenderAccountAsCustomField = true;
    //            envInfo.EventNotification.IncludeSenderAccountAsCustomFieldSpecified = true;
    //            envInfo.EventNotification.IncludeEnvelopeVoidReason = true;
    //            envInfo.EventNotification.IncludeEnvelopeVoidReasonSpecified = true;
    //            envInfo.EventNotification.IncludeDocumentFields = true;
    //            envInfo.EventNotification.IncludeDocumentFieldsSpecified = true;
    //            envInfo.EventNotification.EnvelopeEvents = new EnvelopeEvent[5];
    //            envInfo.EventNotification.EnvelopeEvents[0] = new EnvelopeEvent();
    //            envInfo.EventNotification.EnvelopeEvents[0].EnvelopeEventStatusCode = EnvelopeEventStatusCode.Sent;
    //            envInfo.EventNotification.EnvelopeEvents[1] = new EnvelopeEvent();
    //            envInfo.EventNotification.EnvelopeEvents[1].EnvelopeEventStatusCode = EnvelopeEventStatusCode.Voided;
    //            envInfo.EventNotification.EnvelopeEvents[2] = new EnvelopeEvent();
    //            envInfo.EventNotification.EnvelopeEvents[2].EnvelopeEventStatusCode = EnvelopeEventStatusCode.Delivered;
    //            envInfo.EventNotification.EnvelopeEvents[3] = new EnvelopeEvent();
    //            envInfo.EventNotification.EnvelopeEvents[3].EnvelopeEventStatusCode = EnvelopeEventStatusCode.Declined;
    //            envInfo.EventNotification.EnvelopeEvents[4] = new EnvelopeEvent();
    //            envInfo.EventNotification.EnvelopeEvents[4].EnvelopeEventStatusCode = EnvelopeEventStatusCode.Completed;
    //            envInfo.EventNotification.RecipientEvents = new RecipientEvent[6];
    //            envInfo.EventNotification.RecipientEvents[0] = new RecipientEvent();
    //            envInfo.EventNotification.RecipientEvents[0].RecipientEventStatusCode = RecipientEventStatusCode.AuthenticationFailed;
    //            envInfo.EventNotification.RecipientEvents[1] = new RecipientEvent();
    //            envInfo.EventNotification.RecipientEvents[1].RecipientEventStatusCode = RecipientEventStatusCode.AutoResponded;
    //            envInfo.EventNotification.RecipientEvents[2] = new RecipientEvent();
    //            envInfo.EventNotification.RecipientEvents[2].RecipientEventStatusCode = RecipientEventStatusCode.Completed;
    //            envInfo.EventNotification.RecipientEvents[3] = new RecipientEvent();
    //            envInfo.EventNotification.RecipientEvents[3].RecipientEventStatusCode = RecipientEventStatusCode.Declined;
    //            envInfo.EventNotification.RecipientEvents[4] = new RecipientEvent();
    //            envInfo.EventNotification.RecipientEvents[4].RecipientEventStatusCode = RecipientEventStatusCode.Delivered;
    //            envInfo.EventNotification.RecipientEvents[5] = new RecipientEvent();
    //            envInfo.EventNotification.RecipientEvents[5].RecipientEventStatusCode = RecipientEventStatusCode.Sent;

    //            //Create envelope with all the composite template information 
    //            EnvelopeStatus status = client.CreateEnvelopeFromTemplatesAndForms(envInfo, new CompositeTemplate[] { template }, true);
    //            RequestRecipientTokenAuthenticationAssertion assert = new RequestRecipientTokenAuthenticationAssertion();
    //            assert.AssertionID = "12345";
    //            assert.AuthenticationInstant = DateTime.Now;
    //            assert.AuthenticationMethod = RequestRecipientTokenAuthenticationAssertionAuthenticationMethod.Password;
    //            assert.SecurityDomain = "www.magicparadigm.com";

    //            RequestRecipientTokenClientURLs clientURLs = new RequestRecipientTokenClientURLs();

    //            clientURLs.OnAccessCodeFailed = ConfigurationManager.AppSettings["RecipientTokenClientURLsPrefix"] + "?envelopeId=" + status.EnvelopeID + "&event=OnAccessCodeFailed";
    //            clientURLs.OnCancel = ConfigurationManager.AppSettings["RecipientTokenClientURLsPrefix"] + "?envelopeId=" + status.EnvelopeID + "&event=OnCancel";
    //            clientURLs.OnDecline = ConfigurationManager.AppSettings["RecipientTokenClientURLsPrefix"] + "?envelopeId=" + status.EnvelopeID + "&event=OnDecline";
    //            clientURLs.OnException = ConfigurationManager.AppSettings["RecipientTokenClientURLsPrefix"] + "?envelopeId=" + status.EnvelopeID + "&event=OnException";
    //            clientURLs.OnFaxPending = ConfigurationManager.AppSettings["RecipientTokenClientURLsPrefix"] + "?envelopeId=" + status.EnvelopeID + "&event=OnFaxPending";
    //            clientURLs.OnIdCheckFailed = ConfigurationManager.AppSettings["RecipientTokenClientURLsPrefix"] + "?envelopeId=" + status.EnvelopeID + "&event=OnIdCheckFailed";
    //            clientURLs.OnSessionTimeout = ConfigurationManager.AppSettings["RecipientTokenClientURLsPrefix"] + "?envelopeId=" + status.EnvelopeID + "&event=OnSessionTimeout";
    //            clientURLs.OnTTLExpired = ConfigurationManager.AppSettings["RecipientTokenClientURLsPrefix"] + "?envelopeId=" + status.EnvelopeID + "&event=OnTTLExpired";
    //            clientURLs.OnViewingComplete = ConfigurationManager.AppSettings["RecipientTokenClientURLsPrefix"] + "?envelopeId=" + status.EnvelopeID + "&event=OnViewingComplete";


    //            String url = Request.Url.AbsoluteUri;

    //            String recipientToken;

    //            clientURLs.OnSigningComplete = url.Substring(0, url.LastIndexOf("/")) + "/EmbeddedSigningComplete1.aspx?envelopeID=" + status.EnvelopeID;
    //            recipientToken = client.RequestRecipientToken(status.EnvelopeID, recipients[0].CaptiveInfo.ClientUserId, recipients[0].UserName, recipients[0].Email, assert, clientURLs);
    //            Session["envelopeID"] = status.EnvelopeID;
    //            if (!Request.Browser.Browser.Equals("InternetExplorer") && (!Request.Browser.Browser.Equals("Safari")))
    //            {
    //                docusignFrame.Visible = true;
    //                docusignFrame.Src = recipientToken;
    //            }
    //            else // Handle IE differently since it does not allow dynamic setting of the iFrame width and height
    //            {
    //                docusignFrameIE.Visible = true;
    //                docusignFrameIE.Src = recipientToken;
    //            }


    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        // Log4Net Piece
    //        log4net.ILog logger = log4net.LogManager.GetLogger(typeof(demos_EnvelopeCustom___Document_Fields));
    //        logger.Info("\n----------------------------------------\n");
    //        logger.Error(ex.Message);
    //        logger.Error(ex.StackTrace);
    //        Response.Write(ex.Message);

    //    }
    //    finally
    //    {
    //        if (fs != null)
    //            fs.Close();
    //    }
 
}