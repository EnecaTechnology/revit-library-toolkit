﻿using System.Reflection;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using Nice3point.Revit.Toolkit.Utils;

namespace Nice3point.Revit.Toolkit;

/// <summary>
///     Provides members for setting and retrieving data about Revit application context.
/// </summary>
[PublicAPI]
public static class Context
{
    private static bool _suppressDialogs;
    private static bool _suppressFailures;

    private static bool _suppressFailureErrors;
    private static int? _suppressDialogCode;
    private static Action<DialogBoxShowingEventArgs>? _suppressDialogHandler;

    static Context()
    {
        const BindingFlags staticFlags = BindingFlags.NonPublic | BindingFlags.Static;
        var dbAssembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(assembly => assembly.GetName().Name == "RevitDBAPI");
        ThrowIfNotSupported(dbAssembly);

        var getApplicationMethod = dbAssembly!.ManifestModule.GetMethods(staticFlags).FirstOrDefault(info => info.Name == "RevitApplication.getApplication_");
        ThrowIfNotSupported(getApplicationMethod);

        var proxyType = dbAssembly.DefinedTypes.FirstOrDefault(info => info.FullName == "Autodesk.Revit.Proxy.ApplicationServices.ApplicationProxy");
        ThrowIfNotSupported(proxyType);

        const BindingFlags internalFlags = BindingFlags.NonPublic | BindingFlags.DeclaredOnly | BindingFlags.Instance;
        var proxyConstructor = proxyType!.GetConstructor(internalFlags, null, [getApplicationMethod!.ReturnType], null);
        ThrowIfNotSupported(proxyConstructor);

        var proxy = proxyConstructor!.Invoke([getApplicationMethod.Invoke(null, null)]);
        ThrowIfNotSupported(proxy);

        var applicationType = typeof(Application);
        var applicationConstructor = applicationType.GetConstructor(internalFlags, null, [proxyType], null);
        ThrowIfNotSupported(applicationConstructor);

        var application = (Application)applicationConstructor!.Invoke([proxy]);
        ThrowIfNotSupported(proxy);

        UiApplication = new UIApplication(application);
    }

    /// <summary>
    ///     Represents an active session of the Autodesk Revit user interface, providing access to
    ///     UI customization methods, events, the main window, and the active document.
    /// </summary>
    public static UIApplication UiApplication { get; }

    /// <summary>
    ///     Returns the database level Application represented by this UI level Application
    /// </summary>
    public static Application Application => UiApplication.Application;

    /// <summary>Provides access to an object that represents the currently active project.</summary>
    /// <remarks>External API commands can access this property in read-only mode only!
    /// The ability to modify the property is reserved for future implementations.</remarks>
    /// <exception cref="T:Autodesk.Revit.Exceptions.InvalidOperationException">Thrown when attempting to modify the property.</exception>
    public static UIDocument UiDocument => UiApplication.ActiveUIDocument;

    /// <summary>An object that represents an open Autodesk Revit project.</summary>
    /// <remarks>
    ///     The Document object represents an Autodesk Revit project. Revit can have multiple
    ///     projects open and multiple views to those projects. The active or top most view will be the
    ///     active project and hence the active document which is available from the Application object.
    /// </remarks>
    public static Document Document => UiApplication.ActiveUIDocument.Document;

    /// <summary>The currently active view of the currently active document.</summary>
    /// <since>2012</since>
    /// <remarks>
    ///     <para>
    ///         This property is applicable to the currently active document only.
    ///         Returns <see langword="null" /> if this document doesn't represent the active document.
    ///     </para>
    ///     <para>
    ///         The active view can only be changed when:
    ///         <ul>
    ///             <li>There is no open transaction.</li><li><see cref="P:Autodesk.Revit.DB.Document.IsModifiable" /> is false.</li>
    ///             <li><see cref="P:Autodesk.Revit.DB.Document.IsReadOnly" /> is false.</li>
    ///             <li>ViewActivating, ViewActivated, and any pre-action of events (such as DocumentSaving or DocumentClosing events) are not being handled.</li>
    ///         </ul>
    ///     </para>
    /// </remarks>
    /// <exception cref="T:Autodesk.Revit.Exceptions.ArgumentNullException">
    ///     When setting the property: If the 'view' argument is NULL.
    /// </exception>
    /// <exception cref="T:Autodesk.Revit.Exceptions.ArgumentException">
    ///     When setting the property:
    ///     <ul>
    ///         <li>If the given view is not a valid view of the document; -or-</li><li>If the given view is a template view; -or-</li><li>If the given view is an internal view.</li>
    ///     </ul>
    /// </exception>
    /// <exception cref="T:Autodesk.Revit.Exceptions.InvalidOperationException">
    ///     <para>
    ///         When setting the property:
    ///         <ul>
    ///             <li>If the document is not currently active; -or-</li><li>If the document is currently modifiable (i.e. with an active transaction); -or-</li>
    ///             <li>If the document is currently in read-only state; -or-</li><li>When invoked during either ViewActivating or ViewActivated event; -or-</li>
    ///             <li>When invoked during any pre-action kind of event, such as DocumentSaving, DocumentClosing, etc.</li>
    ///         </ul>
    ///     </para>
    /// </exception>
    public static View? ActiveView
    {
        get => UiApplication.ActiveUIDocument.ActiveView;
        set => UiApplication.ActiveUIDocument.ActiveView = value;
    }

    /// <summary>The currently active graphical view of the currently active document.</summary>
    /// <remarks>
    ///     This property is applicable to the currently active document only.
    ///     Returns <see langword="null" /> if this document doesn't represent the active document.
    /// </remarks>
    public static View? ActiveGraphicalView => UiApplication.ActiveUIDocument.ActiveGraphicalView;

    /// <summary>
    ///     Suppresses the display of the Revit error and warning messages during transaction
    /// </summary>
    /// <param name="resolveErrors">Set <see langword="true"/> if errors should be resolved, otherwise <see langword="false"/> to cancel the transaction</param>
    /// <remarks>
    ///     By default, Revit uses manual error resolution control with user interaction.
    ///     This method provides automatic resolution of all failures without notifying the user or interrupting the program
    /// </remarks>
    public static void SuppressFailures(bool resolveErrors = true)
    {
        if (_suppressFailures)
        {
            _suppressFailureErrors = resolveErrors;
            return;
        }

        _suppressFailures = true;
        _suppressFailureErrors = resolveErrors;
        Application.FailuresProcessing += ResolveFailures;
    }

    /// <summary>
    ///     Suppresses the display of the Revit dialogs
    /// </summary>
    /// <param name="resultCode">The result code you wish the Revit dialog to return</param>
    /// <remarks>
    ///     The range of valid result values depends on the type of dialog as follows:
    ///     <list type="number">
    ///         <item>
    ///             DialogBox: Any non-zero value will cause a dialog to be dismissed.
    ///         </item>
    ///         <item>
    ///             MessageBox: Standard Message Box IDs, such as IDOK and IDCANCEL, are accepted.
    ///             For all possible IDs, refer to the Windows API documentation.
    ///             The ID used must be relevant to the buttons in a message box.
    ///         </item>
    ///         <item>
    ///             TaskDialog: Standard Message Box IDs and Revit Custom IDs are accepted,
    ///             depending on the buttons used in a dialog. Standard buttons, such as OK
    ///             and Cancel, have standard IDs described in Windows API documentation.
    ///             Buttons with custom text have custom IDs with incremental values
    ///             starting at 1001 for the left-most or top-most button in a task dialog.
    ///         </item>
    ///     </list>
    /// </remarks>
    public static void SuppressDialogs(int resultCode = 1)
    {
        if (_suppressDialogs)
        {
            _suppressDialogCode = resultCode;
            return;
        }

        _suppressDialogs = true;
        _suppressDialogCode = resultCode;
        UiApplication.DialogBoxShowing += ResolveDialogBox;
    }

    /// <summary>
    ///     Suppresses the display of the Revit dialogs
    /// </summary>
    /// <param name="handler">Suppress handler</param>
    public static void SuppressDialogs(Action<DialogBoxShowingEventArgs> handler)
    {
        if (_suppressDialogs)
        {
            _suppressDialogHandler = handler;
            return;
        }

        _suppressDialogs = true;
        _suppressDialogHandler = handler;
        UiApplication.DialogBoxShowing += ResolveDialogBox;
    }

    /// <summary>
    ///     Restores display of the Revit dialogs
    /// </summary>
    public static void RestoreDialogs()
    {
        _suppressDialogs = false;
        _suppressDialogCode = null;
        _suppressDialogHandler = null;
        UiApplication.DialogBoxShowing -= ResolveDialogBox;
    }

    /// <summary>
    ///     Restores failure handling
    /// </summary>
    public static void RestoreFailures()
    {
        _suppressFailures = false;
        Application.FailuresProcessing -= ResolveFailures;
    }

    private static void ResolveDialogBox(object? sender, DialogBoxShowingEventArgs args)
    {
        if (_suppressDialogCode.HasValue)
        {
            args.OverrideResult(_suppressDialogCode.Value);
            return;
        }

        _suppressDialogHandler?.Invoke(args);
    }

    private static void ResolveFailures(object? sender, FailuresProcessingEventArgs args)
    {
        var failuresAccessor = args.GetFailuresAccessor();
        var result = _suppressFailureErrors ? FailureUtils.ResolveFailures(failuresAccessor) : FailureUtils.DismissFailures(failuresAccessor);

        args.SetProcessingResult(result);
    }

    [ContractAnnotation("null => halt")]
    private static void ThrowIfNotSupported(object? argument)
    {
        if (argument is null)
        {
            throw new NotSupportedException("The operation is not supported by current Revit API version. Failed to retrieve the application context.");
        }
    }
}