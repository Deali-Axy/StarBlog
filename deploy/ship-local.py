#!/usr/bin/env python3
"""Invoke the requested local ship tool using only checkout-local SSH configuration."""
import ipaddress
import os
from pathlib import Path
import shlex
import sys

root = Path(__file__).resolve().parents[1]
runtime = root / ".ship/local-ssh"
runtime.mkdir(parents=True, exist_ok=True)
config = runtime / "config"
hostname = os.environ.get("STARBLOG_SSH_HOSTNAME", "")
override = ""
if hostname:
    ipaddress.ip_address(hostname)
    override = f"Host deali.cn\n  HostName {hostname}\n  HostKeyAlias tx.deali.cn\n"
config.write_text(override + f"Host *\n  BatchMode yes\n  StrictHostKeyChecking yes\n  ConnectTimeout 15\nInclude {Path.home() / '.ssh/config'}\n")
config.chmod(0o600)
bin_dir = runtime / "bin"
bin_dir.mkdir(exist_ok=True)
for tool in ("ssh", "scp"):
    wrapper = bin_dir / tool
    wrapper.write_text(f"#!/bin/sh\nexec /usr/bin/{tool} -F {shlex.quote(str(config))} \"$@\"\n")
    wrapper.chmod(0o755)
env = os.environ.copy()
env["PATH"] = str(bin_dir) + os.pathsep + env["PATH"]
ship = Path.home() / "code/ship/ship"
os.chdir(root)
os.execve(ship, [str(ship), *sys.argv[1:]], env)
