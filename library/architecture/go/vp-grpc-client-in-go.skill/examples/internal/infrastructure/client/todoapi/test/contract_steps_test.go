package test

import (
	"context"
	"fmt"
	"slices"

	"github.com/bufbuild/protocompile"
	"github.com/cucumber/godog"
	"google.golang.org/protobuf/proto"
	"google.golang.org/protobuf/reflect/protodesc"
	"google.golang.org/protobuf/reflect/protoreflect"
	"google.golang.org/protobuf/types/descriptorpb"

	"github.com/example/todo/gen/client/todo"
	"github.com/example/todo/internal/infrastructure/client/todoapi"
)

// contractRoot is docs/integration/client, the buf module of the consumed
// contracts, from this test's folder; contractFile is the todo API's in it.
const (
	contractRoot = "../../../../../docs/integration/client"
	contractFile = "todo/todo.proto"
)

func registerContractSteps(sc *godog.ScenarioContext) {
	sc.Step(`^the adapter should call exactly these methods, each in the contract:$`, adapterCallsOnly)
	sc.Step(`^the code in gen/client/todo should be generated from docs/integration/client/todo/todo.proto$`, generatedFromContract)
}

// compileContract compiles the copied proto file as it is now, so the
// scenarios read the contract, not the code generated from it some time ago.
func compileContract() (protoreflect.FileDescriptor, error) {
	compiler := protocompile.Compiler{Resolver: &protocompile.SourceResolver{ImportPaths: []string{contractRoot}}}
	files, err := compiler.Compile(context.Background(), contractFile)
	if err != nil {
		return nil, err
	}
	return files[0], nil
}

// adapterCallsOnly compares todoapi.Methods with the table, and finds each of
// them as a method of the copied contract.
func adapterCallsOnly(table *godog.Table) error {
	var want []string
	for _, row := range table.Rows[1:] {
		want = append(want, row.Cells[0].Value)
	}
	if !slices.Equal(todoapi.Methods, want) {
		return fmt.Errorf("the adapter calls %v, want %v", todoapi.Methods, want)
	}
	contract, err := compileContract()
	if err != nil {
		return err
	}
	var methods []string
	services := contract.Services()
	for i := range services.Len() {
		service := services.Get(i)
		for j := range service.Methods().Len() {
			methods = append(methods, fmt.Sprintf("/%s/%s", service.FullName(), service.Methods().Get(j).Name()))
		}
	}
	logf("then: adapter=%v contract=%v", todoapi.Methods, methods)
	for _, method := range todoapi.Methods {
		if !slices.Contains(methods, method) {
			return fmt.Errorf("%s is no method of the contract %v", method, methods)
		}
	}
	return nil
}

// generatedFromContract compares the descriptor compiled into gen/client/todo
// with the copied proto file compiled now. The file options are left out:
// they name the packages of the code generated per language, not what is
// sent, and managed mode of buf.gen.client.yaml replaces them on purpose.
func generatedFromContract() error {
	contract, err := compileContract()
	if err != nil {
		return err
	}
	want := comparable(contract)
	got := comparable(todo.File_todo_todo_proto)
	if !proto.Equal(got, want) {
		return fmt.Errorf("gen/client/todo is not generated from the copied contract; run make proto-gen\n generated: %v\n contract:  %v", got, want)
	}
	return nil
}

func comparable(file protoreflect.FileDescriptor) *descriptorpb.FileDescriptorProto {
	described := protodesc.ToFileDescriptorProto(file)
	described.SourceCodeInfo, described.Options = nil, nil
	return described
}
