/* 
*   Function
*   Copyright © 2024 NatML Inc, The Emscripten Authors. All Rights Reserved.
*/

const DLFCN = {
  
  $LDSO: {
    loadedLibsByName: {},
    loadedLibsByHandle: {}
  },
  $GOT: {},
  $GOTHandler__deps: ["$GOT"],
  $GOTHandler: {
    get:function(obj, symName) {
      if (!GOT[symName]) {
        GOT[symName] = new WebAssembly.Global({'value': 'i32', 'mutable': true});
      }
      return GOT[symName];
    }
  },
  $asmjsMangle: function (x) {
    var unmangledSymbols = ['stackAlloc','stackSave','stackRestore'];
    return x.indexOf('dynCall_') == 0 || unmangledSymbols.includes(x) ? x : '_' + x;
  },
  $mergeLibSymbols__deps: ["$asmjsMangle"],
  $mergeLibSymbols: function(exports, libName) {
    for (var sym in exports) {
      if (!exports.hasOwnProperty(sym)) {
        continue;
      }
      if (!asmLibraryArg.hasOwnProperty(sym) || sym.startsWith("FXN")) {
        asmLibraryArg[sym] = exports[sym];
      }
      var module_sym = asmjsMangle(sym);
      if (!Module.hasOwnProperty(module_sym)) {
        Module[module_sym] = exports[sym];
      }
    }
  },
  $updateTableMap: function(offset, count) {
    if (!functionsInTableMap)
      functionsInTableMap = new WeakMap();
    for (var i = offset; i < offset + count; i++) {
      var item = getWasmTableEntry(i);
      // Ignore null values.
      if (item) {
        functionsInTableMap.set(item, i);
      }
    }
  },
  $isInternalSym: function (symName) {
    // TODO: find a way to mark these in the binary or avoid exporting them.
    return [
      '__cpp_exception',
      '__c_longjmp',
      '__wasm_apply_data_relocs',
      '__dso_handle',
      '__tls_size',
      '__tls_align',
      '__set_stack_limits',
      'emscripten_tls_init',
      '__wasm_init_tls',
      '__wasm_call_ctors',
    ].includes(symName)
    ;
  },
  $updateGOT__deps: ["$isInternalSym", "$GOT"],
  $updateGOT: function (exports, replace) {
    for (var symName in exports) {
      if (isInternalSym(symName)) {
        continue;
      }
      var value = exports[symName];
      if (symName.startsWith('orig$')) {
        symName = symName.split('$')[1];
        replace = true;
      }
      if (!GOT[symName]) {
        GOT[symName] = new WebAssembly.Global({'value': 'i32', 'mutable': true});
      }
      if (replace || GOT[symName].value == 0) {
        if (typeof value == 'function') {
          GOT[symName].value = addFunction(value);
        } else if (typeof value == 'number') {
          GOT[symName].value = value;
        } else if (typeof value == 'bigint') {
          GOT[symName].value = Number(value);
        } else {
          err("unhandled export type for `" + symName + "`: " + (typeof value));
        }
      }
    }
  },
  $relocateExports__deps: ["$updateGOT"],
  $relocateExports: function(exports, memoryBase, replace) {
    var relocated = {};
    for (var e in exports) {
      var value = exports[e];
      if (typeof value == 'object') {
        // a breaking change in the wasm spec, globals are now objects
        // https://github.com/WebAssembly/mutable-global/issues/1
        value = value.value;
      }
      if (typeof value == 'number') {
        value += memoryBase;
      }
      relocated[e] = value;
    }
    updateGOT(relocated, replace);
    return relocated;
  },
  $createInvokeFunction: function (sig) {
    return function() {
      var sp = stackSave();
      try {
        return dynCall(sig, arguments[0], Array.prototype.slice.call(arguments, 1));
      } catch(e) {
        stackRestore(sp);
        if (e !== e+0) throw e;
        _setThrew(1, 0);
      }
    }
  },
  $resolveGlobalSymbol__deps: ["$asmjsMangle", "$createInvokeFunction"],
  $resolveGlobalSymbol: function(symName, direct) {
    var sym;
    if (direct) {
      sym = asmLibraryArg['orig$' + symName];
    }
    if (!sym) {
      sym = asmLibraryArg[symName];
    }
    if (!sym) {
      sym = Module[asmjsMangle(symName)];
    }
    if (symName === "__stack_high")
      sym = _emscripten_stack_get_end();
    if (symName === "__stack_low")
      sym = _emscripten_stack_get_base();
    if (!sym && symName.startsWith('invoke_')) {
      sym = createInvokeFunction(symName.split('_')[1]);
    }
    return sym;
  },
  $getDylinkMetadata: function(binary) {
    var offset = 0;
    var end = 0;
    function getU8() {
      return binary[offset++];
    }
    function getLEB() {
      var ret = 0;
      var mul = 1;
      while (1) {
        var byte = binary[offset++];
        ret += ((byte & 0x7f) * mul);
        mul *= 0x80;
        if (!(byte & 0x80)) break;
      }
      return ret;
    }
    function getString() {
      var len = getLEB();
      offset += len;
      return UTF8ArrayToString(binary, offset - len, len);
    }
    function failIf(condition, message) {
      if (condition) throw new Error(message);
    }
    var name = 'dylink.0';
    if (binary instanceof WebAssembly.Module) {
      var dylinkSection = WebAssembly.Module.customSections(binary, name);
      if (dylinkSection.length === 0) {
        name = 'dylink'
        dylinkSection = WebAssembly.Module.customSections(binary, name);
      }
      failIf(dylinkSection.length === 0, 'need dylink section');
      binary = new Uint8Array(dylinkSection[0]);
      end = binary.length
    } else {
      var int32View = new Uint32Array(new Uint8Array(binary.subarray(0, 24)).buffer);
      var magicNumberFound = int32View[0] == 0x6d736100;
      failIf(!magicNumberFound, 'need to see wasm magic number'); // \0asm
      // we should see the dylink custom section right after the magic number and wasm version
      failIf(binary[8] !== 0, 'need the dylink section to be first')
      offset = 9;
      var section_size = getLEB(); //section size
      end = offset + section_size;
      name = getString();
    }
    var customSection = { neededDynlibs: [], tlsExports: {} };
    if (name == 'dylink') {
      customSection.memorySize = getLEB();
      customSection.memoryAlign = getLEB();
      customSection.tableSize = getLEB();
      customSection.tableAlign = getLEB();
      // shared libraries this module needs. We need to load them first, so that
      // current module could resolve its imports. (see tools/shared.py
      // WebAssembly.make_shared_library() for "dylink" section extension format)
      var neededDynlibsCount = getLEB();
      for (var i = 0; i < neededDynlibsCount; ++i) {
        var libname = getString();
        customSection.neededDynlibs.push(libname);
      }
    } else {
      failIf(name !== 'dylink.0');
      var WASM_DYLINK_MEM_INFO = 0x1;
      var WASM_DYLINK_NEEDED = 0x2;
      var WASM_DYLINK_EXPORT_INFO = 0x3;
      var WASM_SYMBOL_TLS = 0x100;
      while (offset < end) {
        var subsectionType = getU8();
        var subsectionSize = getLEB();
        if (subsectionType === WASM_DYLINK_MEM_INFO) {
          customSection.memorySize = getLEB();
          customSection.memoryAlign = getLEB();
          customSection.tableSize = getLEB();
          customSection.tableAlign = getLEB();
        } else if (subsectionType === WASM_DYLINK_NEEDED) {
          var neededDynlibsCount = getLEB();
          for (var i = 0; i < neededDynlibsCount; ++i) {
            libname = getString();
            customSection.neededDynlibs.push(libname);
          }
        } else if (subsectionType === WASM_DYLINK_EXPORT_INFO) {
          var count = getLEB();
          while (count--) {
            var symname = getString();
            var flags = getLEB();
            if (flags & WASM_SYMBOL_TLS) {
              customSection.tlsExports[symname] = 1;
            }
          }
        } else {
          err('unknown dylink.0 subsection: ' + subsectionType)
          offset += subsectionSize;
        }
      }
    }
    var tableAlign = Math.pow(2, customSection.tableAlign);
    assert(tableAlign === 1, 'invalid tableAlign ' + tableAlign);
    assert(offset == end);
    return customSection;
  },
  $getMemory__deps: ["$GOT"],
  $getMemory: function(size) {
    return _malloc(size);
  },
  $reportUndefinedSymbols__deps: ["$GOT", "$resolveGlobalSymbol"],
  $reportUndefinedSymbols: function () {
    for (var symName in GOT) {
      if (GOT[symName].value == 0) {
        var value = resolveGlobalSymbol(symName, true)
        assert(value, 'undefined symbol `' + symName + '`. perhaps a side module was not linked in? if this global was expected to arrive from a system library, try to build the MAIN_MODULE with EMCC_FORCE_STDLIBS=1 in the environment');
        if (typeof value == 'function') {
          GOT[symName].value = addFunction(value, value.sig);
        } else if (typeof value == 'number') {
          GOT[symName].value = value;
        } else {
          throw new Error('bad export type for `' + symName + '`: ' + (typeof value));
        }
      }
    }
  },

  $loadWebAssemblyModule__deps: ["$getDylinkMetadata", "$loadDynamicLibrary", "$getMemory", "$GOTHandler", "$resolveGlobalSymbol", "$updateTableMap", "$relocateExports", "$reportUndefinedSymbols"],
  $loadWebAssemblyModule: function(binary, flags, handle) {
    var metadata = getDylinkMetadata(binary);
    var originalTable = wasmTable;
    // loadModule loads the wasm module after all its dependencies have been loaded.
    // can be called both sync/async.
    function loadModule() {
      // The first thread to load a given module needs to allocate the static
      // table and memory regions.  Later threads re-use the same table region
      // and can ignore the memory region (since memory is shared between
      // threads already).
      var needsAllocation = !handle || !HEAP8[(((handle)+(24))>>0)];
      if (needsAllocation) {
          // alignments are powers of 2
          var memAlign = Math.pow(2, metadata.memoryAlign);
          // finalize alignments and verify them
          memAlign = Math.max(memAlign, STACK_ALIGN); // we at least need stack alignment
          // prepare memory
          var memoryBase = metadata.memorySize ? alignMemory(getMemory(metadata.memorySize + memAlign), memAlign) : 0; // TODO: add to cleanups
          var tableBase = metadata.tableSize ? wasmTable.length : 0;
          if (handle) {
          HEAP8[(((handle)+(24))>>0)] = 1;
          HEAP32[(((handle)+(28))>>2)] = memoryBase;
          HEAP32[(((handle)+(32))>>2)] = metadata.memorySize;
          HEAP32[(((handle)+(36))>>2)] = tableBase;
          HEAP32[(((handle)+(40))>>2)] = metadata.tableSize;
          }
      } else {
          memoryBase = HEAP32[(((handle)+(28))>>2)];
          tableBase = HEAP32[(((handle)+(36))>>2)];
      }

      var tableGrowthNeeded = tableBase + metadata.tableSize - wasmTable.length;
      if (tableGrowthNeeded > 0) {
        wasmTable.grow(tableGrowthNeeded);
      }

      // This is the export map that we ultimately return.  We declare it here
      // so it can be used within resolveSymbol.  We resolve symbols against
      // this local symbol map in the case there they are not present on the
      // global Module object.  We need this fallback because:
      // a) Modules sometime need to import their own symbols
      // b) Symbols from side modules are not always added to the global namespace.
      var moduleExports;

      function resolveSymbol(sym) {
        var resolved = resolveGlobalSymbol(sym, false);
        if (!resolved) {
        resolved = moduleExports[sym];
        }
        assert(resolved, 'undefined symbol `' + sym + '`. perhaps a side module was not linked in? if this global was expected to arrive from a system library, try to build the MAIN_MODULE with EMCC_FORCE_STDLIBS=1 in the environment');
        return resolved;
      }

      // TODO kill ↓↓↓ (except "symbols local to this module", it will likely be
      // not needed if we require that if A wants symbols from B it has to link
      // to B explicitly: similarly to -Wl,--no-undefined)
      //
      // wasm dynamic libraries are pure wasm, so they cannot assist in
      // their own loading. When side module A wants to import something
      // provided by a side module B that is loaded later, we need to
      // add a layer of indirection, but worse, we can't even tell what
      // to add the indirection for, without inspecting what A's imports
      // are. To do that here, we use a JS proxy (another option would
      // be to inspect the binary directly).

      var proxyHandler = {
          'get': function(stubs, prop) {
          // symbols that should be local to this module
          switch (prop) {
              case '__memory_base': return memoryBase;
              case '__table_base': return tableBase;
              //case "memory": return Module["asm"]["memory"];
              //case "__indirect_function_table": return Module['asm']['__indirect_function_table'];
          }
          if (prop in asmLibraryArg) {
              // No stub needed, symbol already exists in symbol table
              return asmLibraryArg[prop];
          }
          if (prop in Module["asm"]) {
            return Module["asm"][prop];
          }
          // Return a stub function that will resolve the symbol
          // when first called.
          if (!(prop in stubs)) {
              var resolved;
              stubs[prop] = function() {
              if (!resolved) resolved = resolveSymbol(prop);
              return resolved.apply(null, arguments);
              };
          }
          return stubs[prop];
          }
      };
      var proxy = new Proxy({}, proxyHandler);
      var info = {
        'GOT.mem': new Proxy({}, GOTHandler),
        'GOT.func': new Proxy({}, GOTHandler),
        'env': proxy,
        wasi_snapshot_preview1: proxy,
      };

      function postInstantiation(instance) {
        // the table should be unchanged
        assert(wasmTable === originalTable);
        // add new entries to functionsInTableMap
        updateTableMap(tableBase, metadata.tableSize);
        moduleExports = relocateExports(instance.exports, memoryBase);
        if (!flags.allowUndefined) {
          reportUndefinedSymbols();
        }

        // If the runtime has already been initialized we set the stack limits
        // now.  Otherwise this is delayed until `setDylinkStackLimits` is
        // called after initialization.
        if (moduleExports['__set_stack_limits'] && runtimeInitialized) {
          moduleExports['__set_stack_limits'](_emscripten_stack_get_base(), _emscripten_stack_get_end());
        }

        // initialize the module
        var applyRelocs = moduleExports['__wasm_apply_data_relocs'];
        if (applyRelocs) {
          if (runtimeInitialized) {
            applyRelocs();
          } else {
            __RELOC_FUNCS__.push(applyRelocs);
          }
        }
        var init = moduleExports['__wasm_call_ctors'];
        if (init) {
          if (runtimeInitialized) {
            init();
          } else {
            // we aren't ready to run compiled code yet
            __ATINIT__.push(init);
          }
        }
        return moduleExports;
      }

      if (flags.loadAsync) {
        if (binary instanceof WebAssembly.Module) {
          var instance = new WebAssembly.Instance(binary, info);
          return Promise.resolve(postInstantiation(instance));
        }
        return WebAssembly.instantiate(binary, info).then(function(result) {
          return postInstantiation(result.instance);
        });
      }
      var module = binary instanceof WebAssembly.Module ? binary : new WebAssembly.Module(binary);
      var instance = new WebAssembly.Instance(module, info);
      return postInstantiation(instance);
    }
    // now load needed libraries and the module itself.
    if (flags.loadAsync) {
      return metadata.neededDynlibs.reduce(function(chain, dynNeeded) {
        return chain.then(function() {
          return loadDynamicLibrary(dynNeeded, flags);
        });
      }, Promise.resolve()).then(function() {
        return loadModule();
      });
    }
    metadata.neededDynlibs.forEach(function(dynNeeded) {
      if (dynNeeded !== "libFunction.so")
        loadDynamicLibrary(dynNeeded, flags);
    });
    return loadModule();
  },

  $loadDynamicLibrary__deps: ["$LDSO", "$mergeLibSymbols", "$loadWebAssemblyModule"],
    $loadDynamicLibrary: function (lib, flags, handle) {
      if (lib == '__main__' && !LDSO.loadedLibsByName[lib]) {
        LDSO.loadedLibsByName[lib] = {
          refcount: Infinity,   // = nodelete
          name:     '__main__',
          module:   Module['asm'],
          global:   true
        };
      }
      // when loadDynamicLibrary did not have flags, libraries were loaded
      // globally & permanently
      flags = flags || {global: true, nodelete: true}
      var dso = LDSO.loadedLibsByName[lib];
      if (dso) {
        // the library is being loaded or has been loaded already.
        //
        // however it could be previously loaded only locally and if we get
        // load request with global=true we have to make it globally visible now.
        if (flags.global && !dso.global) {
          dso.global = true;
          if (dso.module !== 'loading') {
            // ^^^ if module is 'loading' - symbols merging will be eventually done by the loader.
            mergeLibSymbols(dso.module, lib)
          }
        }
        // same for "nodelete"
        if (flags.nodelete && dso.refcount !== Infinity) {
            dso.refcount = Infinity;
        }
        dso.refcount++
        if (handle) {
            LDSO.loadedLibsByHandle[handle] = dso;
        }
        return flags.loadAsync ? Promise.resolve(true) : true;
      }
      // allocate new DSO
      dso = {
        refcount: flags.nodelete ? Infinity : 1,
        name:     lib,
        module:   'loading',
        global:   flags.global,
      };
      LDSO.loadedLibsByName[lib] = dso;
      if (handle) {
        LDSO.loadedLibsByHandle[handle] = dso;
      }
      // libData <- libFile
      function loadLibData(libFile) {
        // for wasm, we can use fetch for async, but for fs mode we can only imitate it
        if (flags.fs && flags.fs.findObject(libFile)) {
          var libData = flags.fs.readFile(libFile, {encoding: 'binary'});
          if (!(libData instanceof Uint8Array)) {
            libData = new Uint8Array(libData);
          }
          return flags.loadAsync ? Promise.resolve(libData) : libData;
        }
        if (flags.loadAsync) {
          return new Promise(function(resolve, reject) {
            readAsync(libFile, function(data) { resolve(new Uint8Array(data)); }, reject);
          });
        }
        // load the binary synchronously
        if (!readBinary) {
          throw new Error(libFile + ': file not found, and synchronous loading of external files is not available');
        }
        return readBinary(libFile);
      }
      // libModule <- lib
      function getLibModule() {
        // lookup preloaded cache first
        if (Module['preloadedWasm'] !== undefined && Module['preloadedWasm'][lib] !== undefined) {
          var libModule = Module['preloadedWasm'][lib];
          return flags.loadAsync ? Promise.resolve(libModule) : libModule;
        }
        // module not preloaded - load lib data and create new module from it
        if (flags.loadAsync) {
          return loadLibData(lib).then(function(libData) {
            return loadWebAssemblyModule(libData, flags, handle);
          });
        }
        return loadWebAssemblyModule(loadLibData(lib), flags, handle);
      }
      // module for lib is loaded - update the dso & global namespace
      function moduleLoaded(libModule) {
        if (dso.global) {
          mergeLibSymbols(libModule, lib);
        }
        dso.module = libModule;
      }
      if (flags.loadAsync) {
        return getLibModule().then(function(libModule) {
          moduleLoaded(libModule);
          return true;
        });
      }
      moduleLoaded(getLibModule());
      return true;
    },
    $dlSetError: function(msg) {
        withStackSave(function() {
            var cmsg = allocateUTF8OnStack(msg);
            ___dl_seterr(cmsg);
        });
    },
    $dlopenInternal__deps: ["$dlSetError", "$loadDynamicLibrary"],
    $dlopenInternal: function(handle, jsflags) {
        var filename = UTF8ToString(handle + 44);
        var flags = HEAP32[(((handle)+(20))>>2)];
        filename = PATH.normalize(filename);
        var searchpaths = [];
        var isValidFile = (filename) => {
            var target = FS.findObject(filename);
            return target && !target.isFolder && !target.isDevice;
        };
        if (!isValidFile(filename)) {
            if (ENV['LD_LIBRARY_PATH']) {
                searchpaths = ENV['LD_LIBRARY_PATH'].split(':');
            }
            for (var ident in searchpaths) {
                var searchfile = PATH.join2(searchpaths[ident], filename);
                if (isValidFile(searchfile)) {
                    filename = searchfile;
                    break;
                }
            }
        }
        // We don't care about RTLD_NOW and RTLD_LAZY.
        var combinedFlags = {
            global:    Boolean(flags & 256),
            nodelete:  Boolean(flags & 4096),
            loadAsync: jsflags.loadAsync,
            fs:        jsflags.fs,
        }    
        if (jsflags.loadAsync) {
            return loadDynamicLibrary(filename, combinedFlags, handle);
        }
    
        try {
            return loadDynamicLibrary(filename, combinedFlags, handle)
        } catch (e) {
            err('Error in loading dynamic library ' + filename + ": " + e);
            dlSetError('Could not load dynamic lib: ' + filename + '\n' + e);
            return 0;
        }
    },
  _dlopen_js__deps: ["$dlopenInternal"],
  _dlopen_js__sig: "ii",
  _dlopen_js: function(handle) {
    var jsflags = {
      loadAsync: false,
      fs: FS, // load libraries from provided filesystem
    }
    return dlopenInternal(handle, jsflags);
  },
  _dlsym_js__deps: ["$dlSetError", "$LDSO", "$resolveGlobalSymbol"],
  _dlsym_js__sig: "iii",
  _dlsym_js: function(handle, symbol) {
    // void *dlsym(void *restrict handle, const char *restrict name);
    // http://pubs.opengroup.org/onlinepubs/009695399/functions/dlsym.html
    symbol = UTF8ToString(symbol);
    var result;

    if (handle == 0) {
      result = resolveGlobalSymbol(symbol, true);
      if (!result) {
        dlSetError('Tried to lookup unknown symbol "' + symbol + '" in dynamic lib: RTLD_DEFAULT');
        return 0;
      }
    } else {
      var lib = LDSO.loadedLibsByHandle[handle];
      assert(lib, 'Tried to dlsym() from an unopened handle: ' + handle);
      if (!lib.module.hasOwnProperty(symbol)) {
        dlSetError('Tried to lookup unknown symbol "' + symbol + '" in dynamic lib: ' + lib.name)
        return 0;
      }
      result = lib.module['orig$' + symbol];
      if (!result)
        result = lib.module[symbol];
    }

    if (typeof result == 'function') {
      // Insert the function into the wasm table.  If its a direct wasm function
      // the second argument will not be needed.  If its a JS function we rely
      // on the `sig` attribute being set based on the `<func>__sig` specified
      // in library JS file.
      result = addFunction(result, result.sig);
    }
    return result;
  },
};

mergeInto(LibraryManager.library, DLFCN);