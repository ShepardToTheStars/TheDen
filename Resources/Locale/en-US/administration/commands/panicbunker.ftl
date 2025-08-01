# SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 Morb <14136326+Morb0@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 sleepyyapril <123355664+sleepyyapril@users.noreply.github.com>
#
# SPDX-License-Identifier: MIT

cmd-panicbunker-desc = Toggles the panic bunker, which enables stricter restrictions on who's allowed to join the server.
cmd-panicbunker-help = Usage: panicbunker
panicbunker-command-enabled = Panic bunker has been enabled.
panicbunker-command-disabled = Panic bunker has been disabled.

cmd-panicbunker_disable_with_admins-desc = Toggles whether or not the panic bunker will disable when an admin connects.
cmd-panicbunker_disable_with_admins-help = Usage: panicbunker_disable_with_admins
panicbunker-command-disable-with-admins-enabled = The panic bunker will automatically disable with admins online.
panicbunker-command-disable-with-admins-disabled = The panic bunker will not automatically disable with admins online.

cmd-panicbunker_enable_without_admins-desc = Toggles whether or not the panic bunker will enable when the last admin disconnects.
cmd-panicbunker_enable_without_admins-help = Usage: panicbunker_enable_without_admins
panicbunker-command-enable-without-admins-enabled = The panic bunker will automatically enable without admins online.
panicbunker-command-enable-without-admins-disabled = The panic bunker will not automatically enable without admins online.

cmd-panicbunker_count_deadminned_admins-desc = Toggles whether or not to count deadminned admins when automatically enabling and disabling the panic bunker.
cmd-panicbunker_count_deadminned_admins-help = Usage: panicbunker_count_deadminned_admins
panicbunker-command-count-deadminned-admins-enabled = The panic bunker will count deadminned admins when made to automatically enable and disable.
panicbunker-command-count-deadminned-admins-disabled = The panic bunker will not count deadminned admins when made to automatically enable and disable.

cmd-panicbunker_show_reason-desc = Toggles whether or not to show connecting clients the reason why the panic bunker blocked them from joining.
cmd-panicbunker_show_reason-help = Usage: panicbunker_show_reason
panicbunker-command-show-reason-enabled = The panic bunker will now show a reason to users it blocks from connecting.
panicbunker-command-show-reason-disabled = The panic bunker will no longer show a reason to users it blocks from connecting.

cmd-panicbunker_min_account_age-desc = Gets or sets the minimum account age in hours that an account must have to be allowed to connect with the panic bunker enabled.
cmd-panicbunker_min_account_age-help = Usage: panicbunker_min_account_age <hours>
panicbunker-command-min-account-age-is = The minimum account age for the panic bunker is {$hours} hours.
panicbunker-command-min-account-age-set = Set the minimum account age for the panic bunker to {$hours} hours.

cmd-panicbunker_min_overall_hours-desc = Gets or sets the minimum overall playtime in hours that an account must have to be allowed to connect with the panic bunker enabled.
cmd-panicbunker_min_overall_hours-help = Usage: panicbunker_min_overall_hours <hours>
panicbunker-command-min-overall-hours-is = The minimum overall playtime for the panic bunker is {$hours} hours.
panicbunker-command-min-overall-hours-set = Set the minimum overall playtime for the panic bunker to {$hours} hours.
