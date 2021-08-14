﻿//------------------------------------------------------------------------------
// <auto-generated>
//     此代码由工具生成。
//     运行时版本:4.0.30319.42000
//
//     对此文件的更改可能会导致不正确的行为，并且如果
//     重新生成代码，这些更改将会丢失。
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Xml.Serialization;

// 
// 此源代码由 wsdl 自动生成, Version=4.8.3928.0。
// 


/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "4.8.3928.0")]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Web.Services.WebServiceBindingAttribute(Name="AccountServiceSoap", Namespace="http://webservice.com")]
public partial class AccountService : System.Web.Services.Protocols.SoapHttpClientProtocol {
    
    private System.Threading.SendOrPostCallback HelloWorldOperationCompleted;
    
    private System.Threading.SendOrPostCallback GetAccountOperationCompleted;
    
    private System.Threading.SendOrPostCallback GetAccountByNamesOperationCompleted;
    
    private System.Threading.SendOrPostCallback CreateOperationCompleted;
    
    private System.Threading.SendOrPostCallback CreateAccountsOperationCompleted;
    
    private System.Threading.SendOrPostCallback GetAccountsOperationCompleted;
    
    private System.Threading.SendOrPostCallback DeleteOperationCompleted;
    
    /// <remarks/>
    public AccountService() {
        this.Url = "http://localhost:53341/Test.asmx";
    }
    
    /// <remarks/>
    public event HelloWorldCompletedEventHandler HelloWorldCompleted;
    
    /// <remarks/>
    public event GetAccountCompletedEventHandler GetAccountCompleted;
    
    /// <remarks/>
    public event GetAccountByNamesCompletedEventHandler GetAccountByNamesCompleted;
    
    /// <remarks/>
    public event CreateCompletedEventHandler CreateCompleted;
    
    /// <remarks/>
    public event CreateAccountsCompletedEventHandler CreateAccountsCompleted;
    
    /// <remarks/>
    public event GetAccountsCompletedEventHandler GetAccountsCompleted;
    
    /// <remarks/>
    public event DeleteCompletedEventHandler DeleteCompleted;
    
    /// <remarks/>
    [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://webservice.com/HelloWorld", RequestNamespace="http://webservice.com", ResponseNamespace="http://webservice.com", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
    public string HelloWorld() {
        object[] results = this.Invoke("HelloWorld", new object[0]);
        return ((string)(results[0]));
    }
    
    /// <remarks/>
    public System.IAsyncResult BeginHelloWorld(System.AsyncCallback callback, object asyncState) {
        return this.BeginInvoke("HelloWorld", new object[0], callback, asyncState);
    }
    
    /// <remarks/>
    public string EndHelloWorld(System.IAsyncResult asyncResult) {
        object[] results = this.EndInvoke(asyncResult);
        return ((string)(results[0]));
    }
    
    /// <remarks/>
    public void HelloWorldAsync() {
        this.HelloWorldAsync(null);
    }
    
    /// <remarks/>
    public void HelloWorldAsync(object userState) {
        if ((this.HelloWorldOperationCompleted == null)) {
            this.HelloWorldOperationCompleted = new System.Threading.SendOrPostCallback(this.OnHelloWorldOperationCompleted);
        }
        this.InvokeAsync("HelloWorld", new object[0], this.HelloWorldOperationCompleted, userState);
    }
    
    private void OnHelloWorldOperationCompleted(object arg) {
        if ((this.HelloWorldCompleted != null)) {
            System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
            this.HelloWorldCompleted(this, new HelloWorldCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
        }
    }
    
    /// <remarks/>
    [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://webservice.com/GetAccount", RequestNamespace="http://webservice.com", ResponseNamespace="http://webservice.com", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
    public Account GetAccount(System.Guid id) {
        object[] results = this.Invoke("GetAccount", new object[] {
                    id});
        return ((Account)(results[0]));
    }
    
    /// <remarks/>
    public System.IAsyncResult BeginGetAccount(System.Guid id, System.AsyncCallback callback, object asyncState) {
        return this.BeginInvoke("GetAccount", new object[] {
                    id}, callback, asyncState);
    }
    
    /// <remarks/>
    public Account EndGetAccount(System.IAsyncResult asyncResult) {
        object[] results = this.EndInvoke(asyncResult);
        return ((Account)(results[0]));
    }
    
    /// <remarks/>
    public void GetAccountAsync(System.Guid id) {
        this.GetAccountAsync(id, null);
    }
    
    /// <remarks/>
    public void GetAccountAsync(System.Guid id, object userState) {
        if ((this.GetAccountOperationCompleted == null)) {
            this.GetAccountOperationCompleted = new System.Threading.SendOrPostCallback(this.OnGetAccountOperationCompleted);
        }
        this.InvokeAsync("GetAccount", new object[] {
                    id}, this.GetAccountOperationCompleted, userState);
    }
    
    private void OnGetAccountOperationCompleted(object arg) {
        if ((this.GetAccountCompleted != null)) {
            System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
            this.GetAccountCompleted(this, new GetAccountCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
        }
    }
    
    /// <remarks/>
    [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://webservice.com/GetAccountByNames", RequestNamespace="http://webservice.com", ResponseNamespace="http://webservice.com", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
    public Account[] GetAccountByNames(string[] ids) {
        object[] results = this.Invoke("GetAccountByNames", new object[] {
                    ids});
        return ((Account[])(results[0]));
    }
    
    /// <remarks/>
    public System.IAsyncResult BeginGetAccountByNames(string[] ids, System.AsyncCallback callback, object asyncState) {
        return this.BeginInvoke("GetAccountByNames", new object[] {
                    ids}, callback, asyncState);
    }
    
    /// <remarks/>
    public Account[] EndGetAccountByNames(System.IAsyncResult asyncResult) {
        object[] results = this.EndInvoke(asyncResult);
        return ((Account[])(results[0]));
    }
    
    /// <remarks/>
    public void GetAccountByNamesAsync(string[] ids) {
        this.GetAccountByNamesAsync(ids, null);
    }
    
    /// <remarks/>
    public void GetAccountByNamesAsync(string[] ids, object userState) {
        if ((this.GetAccountByNamesOperationCompleted == null)) {
            this.GetAccountByNamesOperationCompleted = new System.Threading.SendOrPostCallback(this.OnGetAccountByNamesOperationCompleted);
        }
        this.InvokeAsync("GetAccountByNames", new object[] {
                    ids}, this.GetAccountByNamesOperationCompleted, userState);
    }
    
    private void OnGetAccountByNamesOperationCompleted(object arg) {
        if ((this.GetAccountByNamesCompleted != null)) {
            System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
            this.GetAccountByNamesCompleted(this, new GetAccountByNamesCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
        }
    }
    
    /// <remarks/>
    [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://webservice.com/Create", RequestNamespace="http://webservice.com", ResponseNamespace="http://webservice.com", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
    public Account Create(Account account) {
        object[] results = this.Invoke("Create", new object[] {
                    account});
        return ((Account)(results[0]));
    }
    
    /// <remarks/>
    public System.IAsyncResult BeginCreate(Account account, System.AsyncCallback callback, object asyncState) {
        return this.BeginInvoke("Create", new object[] {
                    account}, callback, asyncState);
    }
    
    /// <remarks/>
    public Account EndCreate(System.IAsyncResult asyncResult) {
        object[] results = this.EndInvoke(asyncResult);
        return ((Account)(results[0]));
    }
    
    /// <remarks/>
    public void CreateAsync(Account account) {
        this.CreateAsync(account, null);
    }
    
    /// <remarks/>
    public void CreateAsync(Account account, object userState) {
        if ((this.CreateOperationCompleted == null)) {
            this.CreateOperationCompleted = new System.Threading.SendOrPostCallback(this.OnCreateOperationCompleted);
        }
        this.InvokeAsync("Create", new object[] {
                    account}, this.CreateOperationCompleted, userState);
    }
    
    private void OnCreateOperationCompleted(object arg) {
        if ((this.CreateCompleted != null)) {
            System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
            this.CreateCompleted(this, new CreateCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
        }
    }
    
    /// <remarks/>
    [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://webservice.com/CreateAccounts", RequestNamespace="http://webservice.com", ResponseNamespace="http://webservice.com", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
    public Account[] CreateAccounts(Account[] accounts) {
        object[] results = this.Invoke("CreateAccounts", new object[] {
                    accounts});
        return ((Account[])(results[0]));
    }
    
    /// <remarks/>
    public System.IAsyncResult BeginCreateAccounts(Account[] accounts, System.AsyncCallback callback, object asyncState) {
        return this.BeginInvoke("CreateAccounts", new object[] {
                    accounts}, callback, asyncState);
    }
    
    /// <remarks/>
    public Account[] EndCreateAccounts(System.IAsyncResult asyncResult) {
        object[] results = this.EndInvoke(asyncResult);
        return ((Account[])(results[0]));
    }
    
    /// <remarks/>
    public void CreateAccountsAsync(Account[] accounts) {
        this.CreateAccountsAsync(accounts, null);
    }
    
    /// <remarks/>
    public void CreateAccountsAsync(Account[] accounts, object userState) {
        if ((this.CreateAccountsOperationCompleted == null)) {
            this.CreateAccountsOperationCompleted = new System.Threading.SendOrPostCallback(this.OnCreateAccountsOperationCompleted);
        }
        this.InvokeAsync("CreateAccounts", new object[] {
                    accounts}, this.CreateAccountsOperationCompleted, userState);
    }
    
    private void OnCreateAccountsOperationCompleted(object arg) {
        if ((this.CreateAccountsCompleted != null)) {
            System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
            this.CreateAccountsCompleted(this, new CreateAccountsCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
        }
    }
    
    /// <remarks/>
    [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://webservice.com/GetAccounts", RequestNamespace="http://webservice.com", ResponseNamespace="http://webservice.com", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
    public Account[] GetAccounts(System.Guid[] ids) {
        object[] results = this.Invoke("GetAccounts", new object[] {
                    ids});
        return ((Account[])(results[0]));
    }
    
    /// <remarks/>
    public System.IAsyncResult BeginGetAccounts(System.Guid[] ids, System.AsyncCallback callback, object asyncState) {
        return this.BeginInvoke("GetAccounts", new object[] {
                    ids}, callback, asyncState);
    }
    
    /// <remarks/>
    public Account[] EndGetAccounts(System.IAsyncResult asyncResult) {
        object[] results = this.EndInvoke(asyncResult);
        return ((Account[])(results[0]));
    }
    
    /// <remarks/>
    public void GetAccountsAsync(System.Guid[] ids) {
        this.GetAccountsAsync(ids, null);
    }
    
    /// <remarks/>
    public void GetAccountsAsync(System.Guid[] ids, object userState) {
        if ((this.GetAccountsOperationCompleted == null)) {
            this.GetAccountsOperationCompleted = new System.Threading.SendOrPostCallback(this.OnGetAccountsOperationCompleted);
        }
        this.InvokeAsync("GetAccounts", new object[] {
                    ids}, this.GetAccountsOperationCompleted, userState);
    }
    
    private void OnGetAccountsOperationCompleted(object arg) {
        if ((this.GetAccountsCompleted != null)) {
            System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
            this.GetAccountsCompleted(this, new GetAccountsCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
        }
    }
    
    /// <remarks/>
    [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://webservice.com/Delete", RequestNamespace="http://webservice.com", ResponseNamespace="http://webservice.com", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
    public System.Guid Delete(System.Guid id) {
        object[] results = this.Invoke("Delete", new object[] {
                    id});
        return ((System.Guid)(results[0]));
    }
    
    /// <remarks/>
    public System.IAsyncResult BeginDelete(System.Guid id, System.AsyncCallback callback, object asyncState) {
        return this.BeginInvoke("Delete", new object[] {
                    id}, callback, asyncState);
    }
    
    /// <remarks/>
    public System.Guid EndDelete(System.IAsyncResult asyncResult) {
        object[] results = this.EndInvoke(asyncResult);
        return ((System.Guid)(results[0]));
    }
    
    /// <remarks/>
    public void DeleteAsync(System.Guid id) {
        this.DeleteAsync(id, null);
    }
    
    /// <remarks/>
    public void DeleteAsync(System.Guid id, object userState) {
        if ((this.DeleteOperationCompleted == null)) {
            this.DeleteOperationCompleted = new System.Threading.SendOrPostCallback(this.OnDeleteOperationCompleted);
        }
        this.InvokeAsync("Delete", new object[] {
                    id}, this.DeleteOperationCompleted, userState);
    }
    
    private void OnDeleteOperationCompleted(object arg) {
        if ((this.DeleteCompleted != null)) {
            System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
            this.DeleteCompleted(this, new DeleteCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
        }
    }
    
    /// <remarks/>
    public new void CancelAsync(object userState) {
        base.CancelAsync(userState);
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "4.8.3928.0")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://webservice.com")]
public partial class Account {
    
    private System.Guid idField;
    
    private string nameField;
    
    private string eMailField;
    
    private Contact[] contactsField;
    
    /// <remarks/>
    public System.Guid Id {
        get {
            return this.idField;
        }
        set {
            this.idField = value;
        }
    }
    
    /// <remarks/>
    public string Name {
        get {
            return this.nameField;
        }
        set {
            this.nameField = value;
        }
    }
    
    /// <remarks/>
    public string EMail {
        get {
            return this.eMailField;
        }
        set {
            this.eMailField = value;
        }
    }
    
    /// <remarks/>
    public Contact[] Contacts {
        get {
            return this.contactsField;
        }
        set {
            this.contactsField = value;
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "4.8.3928.0")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://webservice.com")]
public partial class Contact {
    
    private int idField;
    
    private string lastNameField;
    
    private string firstNameField;
    
    /// <remarks/>
    public int Id {
        get {
            return this.idField;
        }
        set {
            this.idField = value;
        }
    }
    
    /// <remarks/>
    public string LastName {
        get {
            return this.lastNameField;
        }
        set {
            this.lastNameField = value;
        }
    }
    
    /// <remarks/>
    public string FirstName {
        get {
            return this.firstNameField;
        }
        set {
            this.firstNameField = value;
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "4.8.3928.0")]
public delegate void HelloWorldCompletedEventHandler(object sender, HelloWorldCompletedEventArgs e);

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "4.8.3928.0")]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
public partial class HelloWorldCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
    
    private object[] results;
    
    internal HelloWorldCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
            base(exception, cancelled, userState) {
        this.results = results;
    }
    
    /// <remarks/>
    public string Result {
        get {
            this.RaiseExceptionIfNecessary();
            return ((string)(this.results[0]));
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "4.8.3928.0")]
public delegate void GetAccountCompletedEventHandler(object sender, GetAccountCompletedEventArgs e);

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "4.8.3928.0")]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
public partial class GetAccountCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
    
    private object[] results;
    
    internal GetAccountCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
            base(exception, cancelled, userState) {
        this.results = results;
    }
    
    /// <remarks/>
    public Account Result {
        get {
            this.RaiseExceptionIfNecessary();
            return ((Account)(this.results[0]));
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "4.8.3928.0")]
public delegate void GetAccountByNamesCompletedEventHandler(object sender, GetAccountByNamesCompletedEventArgs e);

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "4.8.3928.0")]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
public partial class GetAccountByNamesCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
    
    private object[] results;
    
    internal GetAccountByNamesCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
            base(exception, cancelled, userState) {
        this.results = results;
    }
    
    /// <remarks/>
    public Account[] Result {
        get {
            this.RaiseExceptionIfNecessary();
            return ((Account[])(this.results[0]));
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "4.8.3928.0")]
public delegate void CreateCompletedEventHandler(object sender, CreateCompletedEventArgs e);

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "4.8.3928.0")]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
public partial class CreateCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
    
    private object[] results;
    
    internal CreateCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
            base(exception, cancelled, userState) {
        this.results = results;
    }
    
    /// <remarks/>
    public Account Result {
        get {
            this.RaiseExceptionIfNecessary();
            return ((Account)(this.results[0]));
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "4.8.3928.0")]
public delegate void CreateAccountsCompletedEventHandler(object sender, CreateAccountsCompletedEventArgs e);

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "4.8.3928.0")]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
public partial class CreateAccountsCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
    
    private object[] results;
    
    internal CreateAccountsCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
            base(exception, cancelled, userState) {
        this.results = results;
    }
    
    /// <remarks/>
    public Account[] Result {
        get {
            this.RaiseExceptionIfNecessary();
            return ((Account[])(this.results[0]));
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "4.8.3928.0")]
public delegate void GetAccountsCompletedEventHandler(object sender, GetAccountsCompletedEventArgs e);

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "4.8.3928.0")]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
public partial class GetAccountsCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
    
    private object[] results;
    
    internal GetAccountsCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
            base(exception, cancelled, userState) {
        this.results = results;
    }
    
    /// <remarks/>
    public Account[] Result {
        get {
            this.RaiseExceptionIfNecessary();
            return ((Account[])(this.results[0]));
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "4.8.3928.0")]
public delegate void DeleteCompletedEventHandler(object sender, DeleteCompletedEventArgs e);

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "4.8.3928.0")]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
public partial class DeleteCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
    
    private object[] results;
    
    internal DeleteCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
            base(exception, cancelled, userState) {
        this.results = results;
    }
    
    /// <remarks/>
    public System.Guid Result {
        get {
            this.RaiseExceptionIfNecessary();
            return ((System.Guid)(this.results[0]));
        }
    }
}
