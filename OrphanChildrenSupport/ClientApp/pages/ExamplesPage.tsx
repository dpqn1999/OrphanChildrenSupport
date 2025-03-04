﻿import "@Styles/main.scss";
import * as React from "react";
import { Helmet } from "react-helmet";
import { RouteComponentProps, withRouter } from "react-router";
import { IPersonModel } from "@Models/IPersonModel";
import * as personStore from "@Store/personStore";
import { withStore } from "@Store/index";
import Paginator from "@Components/shared/Paginator";
import PersonEditor from "@Components/person/PersonEditor";
import AwesomeDebouncePromise from "awesome-debounce-promise";
import { paginate, getPromiseFromActionCreator } from "@Utils";
import { Modal, Button, Container, Row, Card } from "react-bootstrap";
import { wait } from "domain-wait";
import Result from "@Core/Result";

type Props = typeof personStore.actionCreators & personStore.IPersonStoreState & RouteComponentProps<{}>;

interface IState {
    searchTerm: string;
    currentPageNum: number;
    limitPerPage: number;
    isAddModalOpen: boolean;
    isUpdateModalOpen: boolean;
    isDeleteModalOpen: boolean;
    modelForEdit?: IPersonModel;
}

class ExamplesPage extends React.Component<Props, IState> {

    private paginator: Paginator;

    private debouncedSearch: (term: string) => void;

    constructor(props: Props) {
        super(props);

        this.state = {
            searchTerm: "",
            currentPageNum: 1,
            limitPerPage: 5,
            modelForEdit: null,
            isAddModalOpen: false,
            isDeleteModalOpen: false,
            isUpdateModalOpen: false
        };

        // "AwesomeDebouncePromise" makes a delay between
        // the end of input term and search request.
        this.debouncedSearch = AwesomeDebouncePromise((term: string) => {
            props.search(term);
        }, 500);

        wait(async () => {
            // Lets tell Node.js to wait for the request completion.
            // It's necessary when you want to see the fethched data 
            // in your prerendered HTML code (by SSR).
            await this.props.search();
        }, "examplesPageTask");        
    }

    private toggleAddPersonModal = () => {
        this.setState(prev => ({
            isAddModalOpen: !prev.isAddModalOpen
        }));
    }

    private toggleUpdatePersonModal = (modelForEdit?: IPersonModel) => {
        this.setState(prev => ({
            modelForEdit,
            isUpdateModalOpen: !prev.isUpdateModalOpen
        }));
    }

    private toggleDeletePersonModal = (modelForEdit?: IPersonModel) => {
        this.setState(prev => ({
            modelForEdit,
            isDeleteModalOpen: !prev.isDeleteModalOpen
        }));
    }

    private addPerson = async (data: IPersonModel) => {
        var result = await this.props.add(data)as any as Result<number>;

        if (!result.hasErrors) {
            this.paginator.setLastPage();
            this.toggleAddPersonModal();
        }
    }

    private updatePerson = async (data: IPersonModel) => {

        var result = await getPromiseFromActionCreator(this.props.update(data));

        if (!result.hasErrors) {
            this.toggleUpdatePersonModal();
        }
    }

    private deletePerson = (): void => {
        this.props.delete(this.state.modelForEdit.id);
        this.toggleDeletePersonModal();
    }

    private renderRows = (data: IPersonModel[]) => {
        return paginate(data, this.state.currentPageNum, this.state.limitPerPage)
            .map(person =>
                <tr key={person.id}>
                    <td>{person.firstName}</td>
                    <td>{person.lastName}</td>
                    <td>
                        <button className="btn btn-info" onClick={x => this.toggleUpdatePersonModal(person)}>Edit</button>&nbsp;
                    <button className="btn btn-danger" onClick={x => this.toggleDeletePersonModal(person)}>Delete</button>
                    </td>
                </tr>
            );
    }

    private onChangeSearchInput = (e: React.ChangeEvent<HTMLInputElement>) => {
        var val = e.currentTarget.value;
        this.debouncedSearch(val);
        this.paginator.setFirstPage();
    }

    render() {

        return <Container>
            <Helmet>
                <title>Example - OrphanChildrenSupport</title>
            </Helmet>

            <Card body className="mt-4 mb-4">
                <Row>
                    <div className="col-3 col-sm-2 col-md-2 col-lg-1">
                        <button className="btn btn-success" onClick={x => this.toggleAddPersonModal()}>Add</button>
                    </div>
                    <div className="col-9 col-sm-10 col-md-10 col-lg-11">
                        <input
                            type="text"
                            className="form-control"
                            defaultValue={""}
                            onChange={this.onChangeSearchInput}
                            placeholder={"Search for people..."}
                        />
                    </div>
                </Row>
            </Card>

            <table className="table">
                <thead>
                    <tr>
                        <th>First name</th><th>Last name</th><th></th>
                    </tr>
                </thead>
                <tbody>
                    {this.renderRows(this.props.collection)}
                </tbody>
            </table>

            {/* Add modal */}
            <Modal show={this.state.isAddModalOpen} onHide={() => this.toggleAddPersonModal()}>
                <Modal.Header closeButton>
                    <Modal.Title>Add person</Modal.Title>
                </Modal.Header>

                <PersonEditor data={{} as IPersonModel} onSubmit={this.addPerson}>
                    {(renderEditor, handleSubmit) =>
                        <>
                            <Modal.Body>
                                {renderEditor()}
                            </Modal.Body>
                            <Modal.Footer>
                                <Button variant="secondary" onClick={x => this.toggleAddPersonModal()}>Close</Button>
                                <Button variant="primary" onClick={x => handleSubmit()}>Save changes</Button>
                            </Modal.Footer>
                        </>
                    }
                </PersonEditor>
            </Modal>
            
            {/* Update modal */}
            <Modal show={this.state.isUpdateModalOpen} onHide={() => this.toggleUpdatePersonModal()}>
                <Modal.Header closeButton>
                    <Modal.Title>Edit person: {this.state.modelForEdit ? `${this.state.modelForEdit.firstName} ${this.state.modelForEdit.lastName}` : null}</Modal.Title>
                </Modal.Header>

                <PersonEditor data={this.state.modelForEdit} onSubmit={this.updatePerson}>
                    {(renderEditor, handleSubmit) =>
                        <>
                            <Modal.Body>
                                {renderEditor()}
                            </Modal.Body>
                            <Modal.Footer>
                                <Button variant="secondary" onClick={x => this.toggleUpdatePersonModal()}>Close</Button>
                                <Button variant="primary" onClick={x => handleSubmit()}>Save changes</Button>
                            </Modal.Footer>
                        </>
                    }
                </PersonEditor>
            </Modal>

            {/* Delete modal */}
            <Modal show={this.state.isDeleteModalOpen} onHide={() => this.toggleDeletePersonModal()}>
                <Modal.Header closeButton>
                    <Modal.Title>Delete person: {this.state.modelForEdit ? `${this.state.modelForEdit.firstName} ${this.state.modelForEdit.lastName}` : null}</Modal.Title>
                </Modal.Header>
                <Modal.Body>
                    <p>Do you really want to delete this person?</p>
                </Modal.Body>
                <Modal.Footer>
                    <Button variant="secondary" onClick={x => this.toggleDeletePersonModal()}>Close</Button>
                    <Button variant="primary" onClick={x => this.deletePerson()}>Save changes</Button>
                </Modal.Footer>
            </Modal>

            <Paginator
                ref={x => this.paginator = x}
                totalResults={this.props.collection.length}
                limitPerPage={this.state.limitPerPage}
                currentPage={this.state.currentPageNum}
                onChangePage={(pageNum) => this.setState({ currentPageNum: pageNum })} />

        </Container>;
    }
}

// Connect component with Redux store.
var connectedComponent = withStore(
    ExamplesPage,
    state => state.person, // Selects which state properties are merged into the component's props.
    personStore.actionCreators, // Selects which action creators are merged into the component's props.
);

// Attach the React Router to the component to have an opportunity
// to interract with it: use some navigation components, 
// have an access to React Router fields in the component's props, etc.
export default withRouter(connectedComponent);
