import struct, sys, os
# Godot 4 PCK reader (pack format 2/3), unencrypted.
def read(f, fmt):
    s = struct.calcsize('<'+fmt); return struct.unpack('<'+fmt, f.read(s))
def load(path):
    f = open(path,'rb')
    magic = f.read(4)
    if magic != b'GDPC':
        # maybe appended to exe: check tail
        f.seek(-4, 2); 
        if f.read(4) != b'GDPC': raise SystemExit('no GDPC')
        f.seek(-12, 2); (size,) = read(f,'q'); f.seek(-12-size, 2); base = f.tell(); f.seek(base); magic=f.read(4)
    ver, major, minor, patch = read(f,'IIII')
    flags, files_base = read(f,'IQ')
    if ver >= 3:
        (dir_offset,) = read(f,'Q')
        f.seek(dir_offset)
    else:
        f.read(16*4)
    (nfiles,) = read(f,'I')
    entries = []
    for i in range(nfiles):
        (l,) = read(f,'I'); name = f.read(l).rstrip(b'\0').decode('utf-8','replace')
        off, size = read(f,'QQ'); md5 = f.read(16); (fl,) = read(f,'I')
        entries.append((name, files_base+off if (flags & 2) else off, size, fl))
    return f, entries, (ver,major,minor,patch,flags)
if __name__ == '__main__':
    f, entries, meta = load(sys.argv[1])
    cmd = sys.argv[2]
    if cmd == 'list':
        pat = sys.argv[3] if len(sys.argv)>3 else ''
        for n,o,s,fl in entries:
            if pat in n: print(s, n)
    elif cmd == 'extract':
        pat, out = sys.argv[3], sys.argv[4]
        for n,o,s,fl in entries:
            if pat in n:
                p = os.path.join(out, n.replace('res://',''))
                os.makedirs(os.path.dirname(p), exist_ok=True)
                f.seek(o); open(p,'wb').write(f.read(s))
        print('ok')
