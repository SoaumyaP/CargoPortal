/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
/**
 * Specifies whether the Hint or Error messages will be shown.
 *
 * The possible values are:
 *
 * * `initial`(Default) &mdash;Gives an initial, built-in control of the messages.
 * For both Hint and Error components, the configuration depends on the form-bound component state.
 * See [Hint `initial` configuration]({% slug api_inputs_formfieldcomponent %}#toc-showHints) and [Error `initial` configuration]({% slug api_inputs_formfieldcomponent %}#toc-showErrors).
 *
 * * `always`&mdash;Allows full control over the visibility of the messages.
 */
export declare type ShowOptions = 'initial' | 'always';
