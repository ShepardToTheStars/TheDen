# SPDX-FileCopyrightText: 2022 Dylan Corrales <DeathCamel58@gmail.com>
# SPDX-FileCopyrightText: 2022 Moony <moonheart08@users.noreply.github.com>
# SPDX-FileCopyrightText: 2022 Morb <14136326+Morb0@users.noreply.github.com>
# SPDX-FileCopyrightText: 2022 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
# SPDX-FileCopyrightText: 2022 Visne <39844191+Visne@users.noreply.github.com>
# SPDX-FileCopyrightText: 2022 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
# SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
# SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 Kara <lunarautomaton6@gmail.com>
# SPDX-FileCopyrightText: 2023 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
# SPDX-FileCopyrightText: 2023 Vasilis <vasilis@pikachu.systems>
# SPDX-FileCopyrightText: 2024 Chief-Engineer <119664036+Chief-Engineer@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 Simon <63975668+Simyon264@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 nikthechampiongr <32041239+nikthechampiongr@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 sleepyyapril <flyingkarii@gmail.com>
# SPDX-FileCopyrightText: 2025 sleepyyapril <123355664+sleepyyapril@users.noreply.github.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

whitelist-not-whitelisted = You are not whitelisted.

# proper handling for having a min/max or not
whitelist-playercount-invalid = {$min ->
    [0] The whitelist for this server only applies below {$max} players.
    *[other] The whitelist for this server only applies above {$min} {$max ->
        [2147483647] -> players, so you may be able to join later.
       *[other] -> players and below {$max} players, so you may be able to join later.
    }
}
whitelist-not-whitelisted-rp = You are not whitelisted. To become whitelisted, visit our Discord (which can be found at https://denstation.net/discord) and check the #whitelist-info channel.

cmd-whitelistadd-desc = Adds the player with the given username to the server whitelist.
cmd-whitelistadd-help = Usage: whitelistadd <username or User ID>
cmd-whitelistadd-existing = {$username} is already on the whitelist!
cmd-whitelistadd-added = {$username} added to the whitelist
cmd-whitelistadd-not-found = Unable to find '{$username}'
cmd-whitelistadd-arg-player = [player]

cmd-whitelistremove-desc = Removes the player with the given username from the server whitelist.
cmd-whitelistremove-help = Usage: whitelistremove <username or User ID>
cmd-whitelistremove-existing = {$username} is not on the whitelist!
cmd-whitelistremove-removed = {$username} removed from the whitelist
cmd-whitelistremove-not-found = Unable to find '{$username}'
cmd-whitelistremove-arg-player = [player]

cmd-kicknonwhitelisted-desc = Kicks all non-whitelisted players from the server.
cmd-kicknonwhitelisted-help = Usage: kicknonwhitelisted

ban-banned-permanent = This ban will only be removed via appeal.
ban-banned-permanent-appeal = This ban will only be removed via appeal. You can appeal at {$link}
ban-expires = This ban is for {$duration} minutes and will expire at {$time} UTC.
ban-banned-1 = You, or another user of this computer or connection, are banned from playing here.
ban-banned-2 = The ban reason is: "{$reason}"
ban-banned-3 = Attempts to circumvent this ban such as creating a new account will be logged.

soft-player-cap-full = The server is full!
panic-bunker-account-denied = This server is in panic bunker mode, often enabled as a precaution against raids. New connections by accounts not meeting certain requirements are temporarily not accepted. Try again later
panic-bunker-account-denied-reason = This server is in panic bunker mode, often enabled as a precaution against raids. New connections by accounts not meeting certain requirements are temporarily not accepted. Try again later. Reason: "{$reason}"
panic-bunker-account-reason-account = Your Space Station 14 account is too new. It must be older than {$hours} hours
panic-bunker-account-reason-overall = Your overall playtime on the server must be greater than {$hours} hours.

whitelist-playtime = You do not have enough playtime to join this server. You need at least {$hours} minutes of playtime to join this server.
whitelist-player-count = This server is currently not accepting players. Please try again later.
whitelist-notes = You currently have too many admin notes to join this server. You can check your notes by typing /adminremarks in chat.
whitelist-manual = You are not whitelisted on this server.
whitelist-blacklisted = You are blacklisted from this server.
whitelist-always-deny = You are not allowed to join this server.
whitelist-fail-prefix = Not whitelisted: {$msg}
whitelist-misconfigured = The server is misconfigured and is not accepting players. Please contact the server owner and try again later.

cmd-blacklistadd-desc = Adds the player with the given username to the server blacklist.
cmd-blacklistadd-help = Usage: blacklistadd <username>
cmd-blacklistadd-existing = {$username} is already on the blacklist!
cmd-blacklistadd-added = {$username} added to the blacklist
cmd-blacklistadd-not-found = Unable to find '{$username}'
cmd-blacklistadd-arg-player = [player]

cmd-blacklistremove-desc = Removes the player with the given username from the server blacklist.
cmd-blacklistremove-help = Usage: blacklistremove <username>
cmd-blacklistremove-existing = {$username} is not on the blacklist!
cmd-blacklistremove-removed = {$username} removed from the blacklist
cmd-blacklistremove-not-found = Unable to find '{$username}'
cmd-blacklistremove-arg-player = [player]

baby-jail-account-denied = This server is a newbie server, intended for new players and those who want to help them. New connections by accounts that are too old or are not on a whitelist are not accepted. Check out some other servers and see everything Space Station 14 has to offer. Have fun!
baby-jail-account-denied-reason = This server is a newbie server, intended for new players and those who want to help them. New connections by accounts that are too old or are not on a whitelist are not accepted. Check out some other servers and see everything Space Station 14 has to offer. Have fun! Reason: "{$reason}"
baby-jail-account-reason-account = Your Space Station 14 account is too old. It must be younger than {$hours} hours.
baby-jail-account-reason-overall = Your overall playtime on the server must be younger than {$hours} hours.
